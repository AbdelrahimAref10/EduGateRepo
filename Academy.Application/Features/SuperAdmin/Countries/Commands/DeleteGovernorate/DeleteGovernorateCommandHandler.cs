using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteGovernorate;

public sealed class DeleteGovernorateCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteGovernorateCommand, Result>
{
    public async Task<Result> Handle(DeleteGovernorateCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Governorates
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Governorate was not found.");

        if (await dbContext.Cities.AnyAsync(x => x.GovernorateId == request.Id, cancellationToken))
            return Result.Conflict("لا يمكن حذف المحافظة لأنها تحتوي على مدن. احذف المدن أولاً.");

        dbContext.Governorates.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
