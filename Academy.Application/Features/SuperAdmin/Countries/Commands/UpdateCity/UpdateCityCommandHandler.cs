using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateCity;

public sealed class UpdateCityCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateCityCommand, Result<CityDto>>
{
    public async Task<Result<CityDto>> Handle(
        UpdateCityCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Cities
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<CityDto>.NotFound("City was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.Cities
            .AnyAsync(
                x => x.Id != request.Id && x.GovernorateId == entity.GovernorateId && x.NameEn == nameEn,
                cancellationToken);

        if (nameTaken)
            return Result<CityDto>.Conflict("City already exists for this governorate.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;

        await dbContext.SaveChangesAsync(cancellationToken);

        var areasCount = await dbContext.Areas
            .CountAsync(x => x.CityId == entity.Id, cancellationToken);

        return Result<CityDto>.Success(new CityDto
        {
            Id = entity.Id,
            GovernorateId = entity.GovernorateId,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            IsActive = entity.IsActive,
            AreasCount = areasCount
        });
    }
}
