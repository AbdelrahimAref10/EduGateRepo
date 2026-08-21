using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetGovernoratesByCountry;

public sealed class GetGovernoratesByCountryQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetGovernoratesByCountryQuery, Result<IReadOnlyList<GovernorateDto>>>
{
    public async Task<Result<IReadOnlyList<GovernorateDto>>> Handle(
        GetGovernoratesByCountryQuery request,
        CancellationToken cancellationToken)
    {
        var countryExists = await dbContext.Countries
            .AnyAsync(x => x.Id == request.CountryId, cancellationToken);

        if (!countryExists)
            return Result<IReadOnlyList<GovernorateDto>>.NotFound("Country was not found.");

        var query = dbContext.Governorates
            .Where(x => x.CountryId == request.CountryId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;

        var items = await query
            .OrderBy(x => x.NameEn)
            .Select(x => new GovernorateDto
            {
                Id = x.Id,
                CountryId = x.CountryId,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                IsActive = x.IsActive,
                CitiesCount = x.Cities.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GovernorateDto>>.Success(items);
    }
}
