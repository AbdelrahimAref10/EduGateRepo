using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteCountry;

public sealed class DeleteCountryCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteCountryCommand, Result>
{
    public async Task<Result> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Countries
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Country was not found.");

        if (await dbContext.Governorates.AnyAsync(x => x.CountryId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف الدولة لأنها تحتوي على محافظات. احذف المحافظات أولاً.");

        if (await dbContext.Lessons.AnyAsync(x => x.CountryId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف الدولة لأنها مرتبطة بدروس.");

        dbContext.Countries.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
