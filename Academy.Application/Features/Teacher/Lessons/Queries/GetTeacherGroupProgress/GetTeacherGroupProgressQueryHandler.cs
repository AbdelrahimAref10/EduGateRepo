using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.LearningPath.Common;
using Academy.Application.Features.LearningPath.Dtos;
using Academy.Application.Features.Teacher.Students.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetTeacherGroupProgress;

public sealed record GetTeacherGroupProgressQuery(int UserId, int LessonId, int GroupId)
    : IRequest<Result<TeacherGroupProgressDto>>;

public sealed class GetTeacherGroupProgressQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherGroupProgressQuery, Result<TeacherGroupProgressDto>>
{
    public async Task<Result<TeacherGroupProgressDto>> Handle(
        GetTeacherGroupProgressQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherStudentAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<TeacherGroupProgressDto>.NotFound("Teacher profile was not found.");

        var group = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(g =>
                g.Id == request.GroupId
                && g.LessonId == request.LessonId
                && g.Lesson.TeacherId == teacherId.Value)
            .Select(g => new { g.Id, g.Name, g.LessonId, g.Lesson.Subject })
            .FirstOrDefaultAsync(cancellationToken);

        if (group is null)
            return Result<TeacherGroupProgressDto>.NotFound("Group was not found.");

        var members = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(m => m.LessonGroupId == group.Id)
            .Select(m => new { m.StudentId, Name = m.Student.User.FullName })
            .ToListAsync(cancellationToken);

        var studentIds = members.Select(m => m.StudentId).ToList();
        var names = members.ToDictionary(m => m.StudentId, m => m.Name);

        var lessons = await LearningPathQueries.BuildProgressAsync(
            dbContext,
            studentIds,
            names,
            teacherId.Value,
            group.LessonId,
            cancellationToken,
            group.Id);

        return Result<TeacherGroupProgressDto>.Success(new TeacherGroupProgressDto
        {
            LessonId = group.LessonId,
            GroupId = group.Id,
            Subject = group.Subject,
            GroupName = group.Name,
            Members = lessons
        });
    }
}
