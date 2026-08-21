using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYearsByType;

public sealed class GetEducationYearsByTypeQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetEducationYearsByTypeQuery, Result<IReadOnlyList<EducationYearDto>>>
{
    public async Task<Result<IReadOnlyList<EducationYearDto>>> Handle(
        GetEducationYearsByTypeQuery request,
        CancellationToken cancellationToken)
    {
        var typeExists = await dbContext.EducationTypes
            .AnyAsync(x => x.Id == request.EducationTypeId, cancellationToken);

        if (!typeExists)
            return Result<IReadOnlyList<EducationYearDto>>.NotFound("Education type was not found.");

        var query = dbContext.EducationYears
            .Where(x => x.EducationTypeId == request.EducationTypeId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(x => new EducationYearDto
            {
                Id = x.Id,
                EducationTypeId = x.EducationTypeId,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EducationYearDto>>.Success(items);
    }
}
