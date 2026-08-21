using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetAreasByCity;

public sealed class GetAreasByCityQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetAreasByCityQuery, Result<IReadOnlyList<AreaDto>>>
{
    public async Task<Result<IReadOnlyList<AreaDto>>> Handle(
        GetAreasByCityQuery request,
        CancellationToken cancellationToken)
    {
        var cityExists = await dbContext.Cities
            .AnyAsync(x => x.Id == request.CityId, cancellationToken);

        if (!cityExists)
            return Result<IReadOnlyList<AreaDto>>.NotFound("City was not found.");

        var query = dbContext.Areas
            .Where(x => x.CityId == request.CityId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;

        var items = await query
            .OrderBy(x => x.NameEn)
            .Select(x => new AreaDto
            {
                Id = x.Id,
                CityId = x.CityId,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AreaDto>>.Success(items);
    }
}
