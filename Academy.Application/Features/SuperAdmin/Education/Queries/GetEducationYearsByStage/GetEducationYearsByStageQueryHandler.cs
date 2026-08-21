using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYearsByStage;

public sealed class GetEducationYearsByStageQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetEducationYearsByStageQuery, Result<IReadOnlyList<EducationYearDto>>>
{
    public async Task<Result<IReadOnlyList<EducationYearDto>>> Handle(
        GetEducationYearsByStageQuery request,
        CancellationToken cancellationToken)
    {
        var stage = await dbContext.EducationStages
            .Include(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == request.EducationStageId && x.EducationTypeId == request.EducationTypeId,
                cancellationToken);

        if (stage is null)
            return Result<IReadOnlyList<EducationYearDto>>.NotFound("Education stage was not found.");

        var query = dbContext.EducationYears
            .Where(x => x.EducationStageId == request.EducationStageId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;
        var typeName = language == AppLanguage.Arabic
            ? stage.EducationType.NameAr
            : stage.EducationType.NameEn;
        var stageName = language == AppLanguage.Arabic ? stage.NameAr : stage.NameEn;

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(x => new EducationYearDto
            {
                Id = x.Id,
                EducationStageId = x.EducationStageId,
                EducationStageName = stageName,
                EducationTypeId = stage.EducationTypeId,
                EducationTypeName = typeName,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                SubjectsCount = x.Subjects.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EducationYearDto>>.Success(items);
    }
}
