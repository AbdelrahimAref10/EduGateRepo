using Academy.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Common;

internal static class ParentRecipientResolver
{
    /// <summary>Returns UserIds of parent accounts linked to this child student.</summary>
    public static async Task<IReadOnlyList<int>> GetParentUserIdsForChildAsync(
        IApplicationDbContext dbContext,
        int childStudentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ParentChildLinks
            .AsNoTracking()
            .Where(x => x.ChildStudentId == childStudentId)
            .Select(x => x.ParentStudent.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public static async Task<IReadOnlyList<int>> GetParentUserIdsForChildrenAsync(
        IApplicationDbContext dbContext,
        IEnumerable<int> childStudentIds,
        CancellationToken cancellationToken)
    {
        var ids = childStudentIds.Where(id => id > 0).Distinct().ToList();
        if (ids.Count == 0)
            return [];

        return await dbContext.ParentChildLinks
            .AsNoTracking()
            .Where(x => ids.Contains(x.ChildStudentId))
            .Select(x => x.ParentStudent.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
