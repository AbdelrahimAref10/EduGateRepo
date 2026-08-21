using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateCountry;

public sealed class UpdateCountryCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateCountryCommand, Result<CountryDto>>
{
    public async Task<Result<CountryDto>> Handle(
        UpdateCountryCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Countries
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<CountryDto>.NotFound("Country was not found.");

        var code = request.Code.Trim().ToUpperInvariant();

        var codeTaken = await dbContext.Countries
            .AnyAsync(x => x.Id != request.Id && x.Code == code, cancellationToken);

        if (codeTaken)
            return Result<CountryDto>.Conflict("Country code already exists.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = request.NameEn.Trim();
        entity.Code = code;

        await dbContext.SaveChangesAsync(cancellationToken);

        var governoratesCount = await dbContext.Governorates
            .CountAsync(x => x.CountryId == entity.Id, cancellationToken);

        return Result<CountryDto>.Success(new CountryDto
        {
            Id = entity.Id,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Code = entity.Code,
            IsActive = entity.IsActive,
            GovernoratesCount = governoratesCount
        });
    }
}
