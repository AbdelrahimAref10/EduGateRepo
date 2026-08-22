using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom.Exams;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.PublishSessionExam;

public sealed class PublishSessionExamCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<PublishSessionExamCommand, Result<TeacherExamDto>>
{
    public async Task<Result<TeacherExamDto>> Handle(
        PublishSessionExamCommand request,
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

        var exam = await dbContext.Exams
            .AsTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null)
            return Result<TeacherExamDto>.NotFound("لا يوجد امتحان لهذه الحصة.");

        if (exam.Questions.Count == 0)
            return Result<TeacherExamDto>.Failure("الامتحان لا يحتوي على أسئلة.");

        if (exam.Status != ExamStatus.Published)
        {
            exam.Status = ExamStatus.Published;
            exam.PublishedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<TeacherExamDto>.Success(ExamMappings.ToTeacherDto(exam));
    }
}
