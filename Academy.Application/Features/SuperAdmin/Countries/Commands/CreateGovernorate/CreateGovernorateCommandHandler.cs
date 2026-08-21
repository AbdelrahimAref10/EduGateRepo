using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateGovernorate;

public sealed class CreateGovernorateCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateGovernorateCommand, Result<GovernorateDto>>
{
    public async Task<Result<GovernorateDto>> Handle(
        CreateGovernorateCommand request,
        CancellationToken cancellationToken)
    {
        var countryExists = await dbContext.Countries
            .AnyAsync(x => x.Id == request.CountryId && x.IsActive, cancellationToken);

        if (!countryExists)
            return Result<GovernorateDto>.NotFound("Country was not found.");

        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.Governorates
            .AnyAsync(
                x => x.CountryId == request.CountryId && x.NameEn == nameEn,
                cancellationToken);

        if (exists)
            return Result<GovernorateDto>.Conflict("Governorate already exists for this country.");

        var entity = new Governorate
        {
            CountryId = request.CountryId,
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            IsActive = true
        };

        dbContext.Governorates.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<GovernorateDto>.Success(new GovernorateDto
        {
            Id = entity.Id,
            CountryId = entity.CountryId,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            IsActive = entity.IsActive,
            CitiesCount = 0
        });
    }
}
