using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Classroom;

internal static class SessionNumbers
{
    public static Task<int> RankAsync(
        IApplicationDbContext dbContext,
        LessonGroupSession session,
        CancellationToken cancellationToken) =>
        RankAsync(
            dbContext,
            session.LessonGroupId,
            session.SessionDate,
            session.StartTime,
            session.Id,
            cancellationToken);

    public static Task<int> RankAsync(
        IApplicationDbContext dbContext,
        int groupId,
        DateOnly date,
        TimeOnly start,
        int sessionId,
        CancellationToken cancellationToken) =>
        dbContext.LessonGroupSessions.CountAsync(
            s => s.LessonGroupId == groupId
                 && (s.SessionDate < date
                     || (s.SessionDate == date && s.StartTime < start)
                     || (s.SessionDate == date && s.StartTime == start && s.Id <= sessionId)),
            cancellationToken);
}
