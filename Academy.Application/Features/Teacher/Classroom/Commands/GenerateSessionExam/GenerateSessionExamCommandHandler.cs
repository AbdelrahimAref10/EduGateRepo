using Academy.Application.Common.Models;
using Academy.Application.Contracts.Ai;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom.Exams;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.GenerateSessionExam;

public sealed class GenerateSessionExamCommandHandler(
    IApplicationDbContext dbContext,
    IClassroomExamMaterialReader materialReader,
    IAiExamGenerator examGenerator,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GenerateSessionExamCommand, Result<TeacherExamDto>>
{
    public async Task<Result<TeacherExamDto>> Handle(
        GenerateSessionExamCommand request,
        CancellationToken cancellationToken)
    {
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

        var sources = await materialReader.ReadUploadedAsync(request.Files, cancellationToken);
        if (sources.Count == 0)
            return Result<TeacherExamDto>.Failure("تعذر قراءة الملفات المرفوعة. استخدم PDF أو Word أو صورة واضحة.");

        var generated = await examGenerator.GenerateAsync(
            new GenerateExamAiRequest
            {
                Materials = sources,
                QuestionCount = request.QuestionCount,
                Subject = session.LessonGroup.Lesson.Subject,
                Topic = session.Topic,
                Language = requestLanguage.Current
            },
            cancellationToken);

        if (!generated.IsSuccess)
            return Result<TeacherExamDto>.Failure(generated.Error, generated.StatusCode);

        if (existing is not null)
        {
            dbContext.Exams.Remove(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var exam = ExamMappings.ToExamEntity(request.SessionId, request.UserId, generated.Value!);
        dbContext.Exams.Add(exam);
        await dbContext.SaveChangesAsync(cancellationToken);

        var saved = await dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .FirstAsync(x => x.Id == exam.Id, cancellationToken);

        return Result<TeacherExamDto>.Success(ExamMappings.ToTeacherDto(saved));
    }
}
