using Academy.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Common;

internal static class ParentAccess
{
    public static async Task<int?> GetParentStudentIdAsync(
        IApplicationDbContext dbContext,
        int parentUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Students
            .AsNoTracking()
            .Where(x => x.UserId == parentUserId && x.IsParent)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<bool> IsLinkedAsync(
        IApplicationDbContext dbContext,
        int parentStudentId,
        int childStudentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ParentChildLinks
            .AsNoTracking()
            .AnyAsync(
                x => x.ParentStudentId == parentStudentId && x.ChildStudentId == childStudentId,
                cancellationToken);
    }

    public static async Task<IReadOnlyList<int>> GetLinkedChildStudentIdsAsync(
        IApplicationDbContext dbContext,
        int parentStudentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ParentChildLinks
            .AsNoTracking()
            .Where(x => x.ParentStudentId == parentStudentId)
            .Select(x => x.ChildStudentId)
            .ToListAsync(cancellationToken);
    }
}
