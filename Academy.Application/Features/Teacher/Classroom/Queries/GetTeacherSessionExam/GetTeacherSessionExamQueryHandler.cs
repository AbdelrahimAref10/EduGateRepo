using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom.Exams;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherSessionExam;

public sealed class GetTeacherSessionExamQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherSessionExamQuery, Result<TeacherExamDto?>>
{
    public async Task<Result<TeacherExamDto?>> Handle(
        GetTeacherSessionExamQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<TeacherExamDto?>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<TeacherExamDto?>.NotFound("الحصة غير موجودة.");

        var exam = await dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        return Result<TeacherExamDto?>.Success(
            exam is null ? null : ExamMappings.ToTeacherDto(exam));
    }
}
