using Academy.Application.Common.Models;
using Academy.Application.Contracts.Ai;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom.Exams;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.GenerateSessionExam;

public sealed class GenerateSessionExamCommandHandler(
    IApplicationDbContext dbContext,
    IClassroomExamMaterialReader materialReader,
    IAiExamGenerator examGenerator,
    IExamGenerationProgress progress,
    INotificationService notificationService,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GenerateSessionExamCommand, Result<TeacherExamDto>>
{
    public async Task<Result<TeacherExamDto>> Handle(
        GenerateSessionExamCommand request,
        CancellationToken cancellationToken)
    {
        await progress.ReportAsync(request.UserId, ExamGenerationSteps.Read(), cancellationToken);

        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<TeacherExamDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<TeacherExamDto>.NotFound("الحصة غير موجودة.");

        var existing = await dbContext.Exams
            .AsTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Attempts)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (existing is not null && existing.Attempts.Count > 0)
            return Result<TeacherExamDto>.Conflict("لا يمكن إعادة التوليد بعد أن بدأ الطلاب الحل.");

        await progress.ReportAsync(request.UserId, ExamGenerationSteps.Prepare(), cancellationToken);

        var sources = await materialReader.ReadUploadedAsync(request.Files, cancellationToken);
        if (sources.Count == 0)
            return Result<TeacherExamDto>.Failure("تعذر قراءة الملفات المرفوعة. استخدم PDF أو Word أو صورة واضحة.");

        var generated = await examGenerator.GenerateAsync(
            new GenerateExamAiRequest
            {
                UserId = request.UserId,
                Materials = sources,
                QuestionCount = request.QuestionCount,
                Subject = session.LessonGroup.Lesson.Subject,
                Topic = session.Topic,
                Language = requestLanguage.Current
            },
            cancellationToken);

        if (!generated.IsSuccess)
            return Result<TeacherExamDto>.Failure(generated.Error, generated.StatusCode);

        await progress.ReportAsync(request.UserId, ExamGenerationSteps.Save(), cancellationToken);

        if (existing is not null)
        {
            dbContext.Exams.Remove(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var exam = ExamMappings.ToExamEntity(
            request.SessionId,
            request.UserId,
            generated.Value!,
            request.MinutesPerQuestion * 60);
        dbContext.Exams.Add(exam);
        await dbContext.SaveChangesAsync(cancellationToken);

        var saved = await dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .FirstAsync(x => x.Id == exam.Id, cancellationToken);

        await progress.ReportAsync(request.UserId, ExamGenerationSteps.Done(), cancellationToken);

        var studentUserIds = await dbContext.LessonGroupMembers
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .Select(x => x.Student.UserId)
            .ToListAsync(cancellationToken);

        if (studentUserIds.Count > 0)
        {
            var teacherName = session.LessonGroup.Lesson.Teacher.User.FullName;
            await notificationService.CreateAsync(
                new NotificationCreateRequest
                {
                    RecipientUserIds = studentUserIds,
                    UserTargetId = request.UserId,
                    Type = NotificationType.ExamPublished,
                    EntityType = NotificationEntityType.Session,
                    EntityId = request.SessionId,
                    TitleAr = "امتحان جديد",
                    TitleEn = "New exam",
                    BodyAr = $"المعلم {teacherName} أنشأ امتحان «{saved.Title}». ادخل وحله.",
                    BodyEn = $"Teacher {teacherName} created the exam '{saved.Title}'. Open it and start.",
                    IncludeSuperAdmins = false
                },
                cancellationToken);
        }

        return Result<TeacherExamDto>.Success(ExamMappings.ToTeacherDto(saved));
    }
}
