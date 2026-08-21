using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetCountries;

public sealed class GetCountriesQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetCountriesQuery, Result<IReadOnlyList<CountryDto>>>
{
    public async Task<Result<IReadOnlyList<CountryDto>>> Handle(
        GetCountriesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Country> query = dbContext.Countries;

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;

        var items = await query
            .OrderBy(x => x.NameEn)
            .Select(x => new CountryDto
            {
                Id = x.Id,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                Code = x.Code,
                IsActive = x.IsActive,
                GovernoratesCount = x.Governorates.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CountryDto>>.Success(items);
    }
}
