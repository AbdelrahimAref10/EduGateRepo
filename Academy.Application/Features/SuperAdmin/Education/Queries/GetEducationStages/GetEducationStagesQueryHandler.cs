using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationStages;

public sealed class GetEducationStagesQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetEducationStagesQuery, Result<IReadOnlyList<EducationStageDto>>>
{
    public async Task<Result<IReadOnlyList<EducationStageDto>>> Handle(
        GetEducationStagesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<EducationStage> query = dbContext.EducationStages;

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(x => new EducationStageDto
            {
                Id = x.Id,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                YearsCount = x.Years.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EducationStageDto>>.Success(items);
    }
}
