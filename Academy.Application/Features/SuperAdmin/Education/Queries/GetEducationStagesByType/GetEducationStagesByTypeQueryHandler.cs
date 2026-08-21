using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationStagesByType;

public sealed class GetEducationStagesByTypeQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetEducationStagesByTypeQuery, Result<IReadOnlyList<EducationStageDto>>>
{
    public async Task<Result<IReadOnlyList<EducationStageDto>>> Handle(
        GetEducationStagesByTypeQuery request,
        CancellationToken cancellationToken)
    {
        var type = await dbContext.EducationTypes
            .FirstOrDefaultAsync(x => x.Id == request.EducationTypeId, cancellationToken);

        if (type is null)
            return Result<IReadOnlyList<EducationStageDto>>.NotFound("Education type was not found.");

        var query = dbContext.EducationStages
            .Where(x => x.EducationTypeId == request.EducationTypeId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;
        var typeName = language == AppLanguage.Arabic ? type.NameAr : type.NameEn;

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(x => new EducationStageDto
            {
                Id = x.Id,
                EducationTypeId = x.EducationTypeId,
                EducationTypeName = typeName,
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
