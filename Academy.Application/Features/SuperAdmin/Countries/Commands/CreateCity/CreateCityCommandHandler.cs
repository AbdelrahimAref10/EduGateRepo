using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateCity;

public sealed class CreateCityCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateCityCommand, Result<CityDto>>
{
    public async Task<Result<CityDto>> Handle(
        CreateCityCommand request,
        CancellationToken cancellationToken)
    {
        var governorateExists = await dbContext.Governorates
            .AnyAsync(x => x.Id == request.GovernorateId && x.IsActive, cancellationToken);

        if (!governorateExists)
            return Result<CityDto>.NotFound("Governorate was not found.");

        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.Cities
            .AnyAsync(
                x => x.GovernorateId == request.GovernorateId && x.NameEn == nameEn,
                cancellationToken);

        if (exists)
            return Result<CityDto>.Conflict("City already exists for this governorate.");

        var entity = new City
        {
            GovernorateId = request.GovernorateId,
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            IsActive = true
        };

        dbContext.Cities.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CityDto>.Success(new CityDto
        {
            Id = entity.Id,
            GovernorateId = entity.GovernorateId,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            IsActive = entity.IsActive,
            AreasCount = 0
        });
    }
}
