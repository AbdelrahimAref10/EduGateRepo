using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateCountry;

public sealed class CreateCountryCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateCountryCommand, Result<CountryDto>>
{
    public async Task<Result<CountryDto>> Handle(
        CreateCountryCommand request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        var exists = await dbContext.Countries
            .AnyAsync(x => x.Code == code, cancellationToken);

        if (exists)
            return Result<CountryDto>.Conflict("Country code already exists.");

        var country = new Country
        {
            NameAr = request.NameAr.Trim(),
            NameEn = request.NameEn.Trim(),
            Code = code,
            IsActive = true
        };

        dbContext.Countries.Add(country);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CountryDto>.Success(new CountryDto
        {
            Id = country.Id,
            Name = LocalizedNames.Pick(country.NameAr, country.NameEn, requestLanguage.Current),
            NameAr = country.NameAr,
            NameEn = country.NameEn,
            Code = country.Code,
            IsActive = country.IsActive,
            GovernoratesCount = 0
        });
    }
}
