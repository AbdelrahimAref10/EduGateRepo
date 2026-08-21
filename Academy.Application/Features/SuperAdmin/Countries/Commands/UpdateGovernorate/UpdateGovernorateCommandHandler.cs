using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateGovernorate;

public sealed class UpdateGovernorateCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateGovernorateCommand, Result<GovernorateDto>>
{
    public async Task<Result<GovernorateDto>> Handle(
        UpdateGovernorateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Governorates
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<GovernorateDto>.NotFound("Governorate was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.Governorates
            .AnyAsync(
                x => x.Id != request.Id && x.CountryId == entity.CountryId && x.NameEn == nameEn,
                cancellationToken);

        if (nameTaken)
            return Result<GovernorateDto>.Conflict("Governorate already exists for this country.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;

        await dbContext.SaveChangesAsync(cancellationToken);

        var citiesCount = await dbContext.Cities
            .CountAsync(x => x.GovernorateId == entity.Id, cancellationToken);

        return Result<GovernorateDto>.Success(new GovernorateDto
        {
            Id = entity.Id,
            CountryId = entity.CountryId,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            IsActive = entity.IsActive,
            CitiesCount = citiesCount
        });
    }
}
