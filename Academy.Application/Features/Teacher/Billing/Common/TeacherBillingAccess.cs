using Academy.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Common;

internal static class TeacherBillingAccess
{
    public static async Task<int?> GetTeacherIdAsync(
        IApplicationDbContext dbContext,
        int userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
