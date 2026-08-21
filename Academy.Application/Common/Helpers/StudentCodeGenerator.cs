using Academy.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Common.Helpers;

public static class StudentCodeGenerator
{
    public static async Task<string> GenerateUniqueAsync(
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = $"STU{Random.Shared.Next(10000000, 99999999)}";
            var exists = await dbContext.Students
                .AnyAsync(x => x.StudentCode == code, cancellationToken);

            if (!exists)
                return code;
        }

        throw new InvalidOperationException("Unable to generate a unique student code.");
    }
}
