using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Commands.SubmitStudentSessionExam;

public sealed class SubmitStudentSessionExamCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<SubmitStudentSessionExamCommand, Result<StudentExamDto>>
{
    public async Task<Result<StudentExamDto>> Handle(
        SubmitStudentSessionExamCommand request,
        CancellationToken cancellationToken)
    {
        var access = await StudentExamAccess.ResolveAsync(dbContext, request.UserId, request.SessionId, cancellationToken);
        if (!access.IsSuccess)
            return Result<StudentExamDto>.Failure(access.Error, access.StatusCode);

        var exam = await dbContext.Exams
            .AsTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Attempts.Where(a => a.StudentId == access.Value!.StudentId))
                .ThenInclude(a => a.Answers)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null || exam.Status != ExamStatus.Published)
            return Result<StudentExamDto>.NotFound("الامتحان غير متاح بعد.");

        var attempt = exam.Attempts.FirstOrDefault();
        if (attempt is null)
            return Result<StudentExamDto>.Conflict("ابدأ الامتحان أولاً.");

        var alreadySubmitted = attempt.SubmittedAtUtc.HasValue;

        await StudentExamProgress.ApplyExpiredQuestionsAsync(dbContext, exam, attempt, cancellationToken);

        if (attempt.SubmittedAtUtc.HasValue)
        {
            if (!alreadySubmitted)
                await NotifyTeacherAsync(access.Value!.StudentId, exam, attempt, cancellationToken);

            return Result<StudentExamDto>.Success(StudentExamProgress.ToDto(exam, attempt));
        }

        var questions = exam.Questions.OrderBy(q => q.SortOrder).ThenBy(q => q.Id).ToList();
        if (attempt.CurrentQuestionIndex < 0 || attempt.CurrentQuestionIndex >= questions.Count)
        {
            StudentExamProgress.Complete(attempt, questions);
            await dbContext.SaveChangesAsync(cancellationToken);
            if (!alreadySubmitted)
                await NotifyTeacherAsync(access.Value!.StudentId, exam, attempt, cancellationToken);
            return Result<StudentExamDto>.Success(StudentExamProgress.ToDto(exam, attempt));
        }

        var question = questions[attempt.CurrentQuestionIndex];
        var alreadyAnswered = attempt.Answers.Any(x => x.ExamQuestionId == question.Id);
        if (!alreadyAnswered)
        {
            var timedOut = StudentExamProgress.RemainingSeconds(exam, attempt) <= 0;
            var option = timedOut || request.OptionId is null
                ? null
                : question.Options.FirstOrDefault(o => o.Id == request.OptionId);

            attempt.Answers.Add(new ExamAttemptAnswer
            {
                ExamQuestionId = question.Id,
                SelectedOptionId = option?.Id,
                IsCorrect = option?.IsCorrect == true
            });
        }

        attempt.CurrentQuestionIndex++;
        attempt.CurrentQuestionStartedAtUtc = DateTime.UtcNow;

        if (attempt.CurrentQuestionIndex >= questions.Count)
            StudentExamProgress.Complete(attempt, questions);

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!alreadySubmitted && attempt.SubmittedAtUtc.HasValue)
            await NotifyTeacherAsync(access.Value!.StudentId, exam, attempt, cancellationToken);

        return Result<StudentExamDto>.Success(StudentExamProgress.ToDto(exam, attempt));
    }

    private async Task NotifyTeacherAsync(
        int studentId,
        Exam exam,
        ExamAttempt attempt,
        CancellationToken cancellationToken)
    {
        var teacherUserId = await dbContext.Exams
            .Where(x => x.Id == exam.Id)
            .Select(x => x.LessonGroupSession.LessonGroup.Lesson.Teacher.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        var student = await dbContext.Students
            .Where(x => x.Id == studentId)
            .Select(x => new { x.UserId, Name = x.User.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherUserId <= 0 || student is null)
            return;

        var max = attempt.MaxScore > 0 ? attempt.MaxScore : 0;
        var percent = max > 0 ? Math.Round(attempt.Score * 100m / max, 1) : 0;

        await notificationService.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [teacherUserId],
                UserTargetId = student.UserId,
                Type = NotificationType.StudentExamSubmitted,
                EntityType = NotificationEntityType.Session,
                EntityId = exam.LessonGroupSessionId,
                TitleAr = "طالب أنهى الامتحان",
                TitleEn = "Student finished an exam",
                BodyAr = $"الطالب {student.Name} أنهى امتحان «{exam.Title}» بنتيجة {attempt.Score}/{max} ({percent}%).",
                BodyEn = $"Student {student.Name} finished the exam '{exam.Title}' with {attempt.Score}/{max} ({percent}%).",
                IncludeSuperAdmins = false
            },
            cancellationToken);

        await ParentNotifications.NotifyLinkedParentsAsync(
            dbContext,
            notificationService,
            studentId,
            new NotificationCreateRequest
            {
                RecipientUserIds = [],
                UserTargetId = student.UserId,
                Type = NotificationType.StudentExamSubmitted,
                EntityType = NotificationEntityType.Session,
                EntityId = exam.LessonGroupSessionId,
                TitleAr = "نتيجة امتحان جاهزة",
                TitleEn = "Exam score ready",
                BodyAr = $"أنهى {student.Name} امتحان «{exam.Title}» بنتيجة {attempt.Score}/{max} ({percent}%).",
                BodyEn = $"{student.Name} finished the exam '{exam.Title}' with {attempt.Score}/{max} ({percent}%).",
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }
}
