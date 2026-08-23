using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Groups;

internal static class AdminSessionAccess
{
    public static Task<LessonGroupSession?> LoadSessionAsync(
        IApplicationDbContext dbContext,
        int sessionId,
        CancellationToken cancellationToken) =>
        dbContext.LessonGroupSessions
            .AsNoTracking()
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
                    .ThenInclude(x => x.Teacher)
                        .ThenInclude(x => x.User)
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
                    .ThenInclude(x => x.EducationSubject)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
}
