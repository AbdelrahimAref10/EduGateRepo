using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteCity;

public sealed class DeleteCityCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteCityCommand, Result>
{
    public async Task<Result> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Cities
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("City was not found.");

        if (await dbContext.Areas.AnyAsync(x => x.CityId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف المدينة لأنها تحتوي على مناطق. احذف المناطق أولاً.");

        dbContext.Cities.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
