using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroupSessions;

public sealed class GetLessonGroupSessionsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLessonGroupSessionsQuery, Result<IReadOnlyList<LessonGroupSessionDto>>>
{
    public async Task<Result<IReadOnlyList<LessonGroupSessionDto>>> Handle(
        GetLessonGroupSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LessonGroupSessionDto>>.NotFound("Teacher profile was not found.");

        var exists = await dbContext.LessonGroups
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.GroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.TeacherId == teacherId,
                cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<LessonGroupSessionDto>>.NotFound("Group was not found.");

        var sessions = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.LessonGroupId == request.GroupId)
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ThenBy(x => x.Id)
            .Select(x => new LessonGroupSessionDto
            {
                Id = x.Id,
                LessonGroupId = x.LessonGroupId,
                SessionDate = x.SessionDate,
                StartTime = x.StartTime,
                Topic = x.Topic,
                Description = x.Description,
                StartedAtUtc = x.StartedAtUtc,
                EndedAtUtc = x.EndedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                HasEnded = x.EndedAtUtc != null,
                CanStart = x.StartedAtUtc == null
                    && x.EndedAtUtc == null
                    && x.LessonGroup.EndedAtUtc == null,
                CanOpenClassroom = x.StartedAtUtc != null,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LessonGroupSessionDto>>.Success(sessions);
    }
}
