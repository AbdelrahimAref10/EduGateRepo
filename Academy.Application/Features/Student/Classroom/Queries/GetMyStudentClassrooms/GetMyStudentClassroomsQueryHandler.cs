using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetMyStudentClassrooms;

public sealed class GetMyStudentClassroomsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyStudentClassroomsQuery, Result<IReadOnlyList<StudentClassroomSessionListItemDto>>>
{
    public async Task<Result<IReadOnlyList<StudentClassroomSessionListItemDto>>> Handle(
        GetMyStudentClassroomsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<IReadOnlyList<StudentClassroomSessionListItemDto>>.NotFound(
                "Student profile was not found.");

        var sessions = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.StartedAtUtc != null
                        && x.LessonGroup.Members.Any(m => m.StudentId == student.Id))
            .OrderByDescending(x => x.SessionDate)
            .ThenByDescending(x => x.StartTime)
            .Select(x => new StudentClassroomSessionListItemDto
            {
                SessionId = x.Id,
                LessonId = x.LessonGroup.LessonId,
                LessonGroupId = x.LessonGroupId,
                GroupName = x.LessonGroup.Name,
                Subject = x.LessonGroup.Lesson.Subject,
                SessionDate = x.SessionDate,
                StartTime = x.StartTime,
                Topic = x.Topic,
                HasEnded = x.EndedAtUtc != null,
                StartedAtUtc = x.StartedAtUtc,
                EndedAtUtc = x.EndedAtUtc,
                TeacherName = (x.LessonGroup.Lesson.Teacher.User.FirstName + " "
                               + x.LessonGroup.Lesson.Teacher.User.LastName).Trim(),
                CanOpenClassroom = x.StartedAtUtc != null
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StudentClassroomSessionListItemDto>>.Success(sessions);
    }
}
