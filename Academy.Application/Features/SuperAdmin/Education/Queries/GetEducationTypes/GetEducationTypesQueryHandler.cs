using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationTypes;

public sealed class GetEducationTypesQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetEducationTypesQuery, Result<IReadOnlyList<EducationTypeDto>>>
{
    public async Task<Result<IReadOnlyList<EducationTypeDto>>> Handle(
        GetEducationTypesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<EducationType> query = dbContext.EducationTypes;

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(x => new EducationTypeDto
            {
                Id = x.Id,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                StagesCount = x.Stages.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EducationTypeDto>>.Success(items);
    }
}
