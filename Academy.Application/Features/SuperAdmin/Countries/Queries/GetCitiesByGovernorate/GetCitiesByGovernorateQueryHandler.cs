using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetCitiesByGovernorate;

public sealed class GetCitiesByGovernorateQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetCitiesByGovernorateQuery, Result<IReadOnlyList<CityDto>>>
{
    public async Task<Result<IReadOnlyList<CityDto>>> Handle(
        GetCitiesByGovernorateQuery request,
        CancellationToken cancellationToken)
    {
        var governorateExists = await dbContext.Governorates
            .AnyAsync(x => x.Id == request.GovernorateId, cancellationToken);

        if (!governorateExists)
            return Result<IReadOnlyList<CityDto>>.NotFound("Governorate was not found.");

        var query = dbContext.Cities
            .Where(x => x.GovernorateId == request.GovernorateId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;

        var items = await query
            .OrderBy(x => x.NameEn)
            .Select(x => new CityDto
            {
                Id = x.Id,
                GovernorateId = x.GovernorateId,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                IsActive = x.IsActive,
                AreasCount = x.Areas.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CityDto>>.Success(items);
    }
}
