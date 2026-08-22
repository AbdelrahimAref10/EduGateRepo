using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Commands.SubmitStudentSessionExam;

public sealed class SubmitStudentSessionExamCommandHandler(IApplicationDbContext dbContext)
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

        await StudentExamProgress.ApplyExpiredQuestionsAsync(dbContext, exam, attempt, cancellationToken);

        if (attempt.SubmittedAtUtc.HasValue)
            return Result<StudentExamDto>.Success(StudentExamProgress.ToDto(exam, attempt));

        var questions = exam.Questions.OrderBy(q => q.SortOrder).ThenBy(q => q.Id).ToList();
        if (attempt.CurrentQuestionIndex < 0 || attempt.CurrentQuestionIndex >= questions.Count)
        {
            StudentExamProgress.Complete(attempt, questions);
            await dbContext.SaveChangesAsync(cancellationToken);
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
        return Result<StudentExamDto>.Success(StudentExamProgress.ToDto(exam, attempt));
    }
}
