using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationSubjectsByYear;

public sealed class GetEducationSubjectsByYearQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetEducationSubjectsByYearQuery, Result<IReadOnlyList<EducationSubjectDto>>>
{
    public async Task<Result<IReadOnlyList<EducationSubjectDto>>> Handle(
        GetEducationSubjectsByYearQuery request,
        CancellationToken cancellationToken)
    {
        var year = await dbContext.EducationYears
            .Include(x => x.EducationStage)
                .ThenInclude(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == request.EducationYearId
                    && x.EducationStageId == request.EducationStageId
                    && x.EducationStage.EducationTypeId == request.EducationTypeId,
                cancellationToken);

        if (year is null)
            return Result<IReadOnlyList<EducationSubjectDto>>.NotFound("Education year was not found.");

        var query = dbContext.EducationSubjects
            .Where(x => x.EducationYearId == request.EducationYearId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var language = requestLanguage.Current;
        var yearName = language == AppLanguage.Arabic ? year.NameAr : year.NameEn;
        var stageName = language == AppLanguage.Arabic
            ? year.EducationStage.NameAr
            : year.EducationStage.NameEn;
        var typeName = language == AppLanguage.Arabic
            ? year.EducationStage.EducationType.NameAr
            : year.EducationStage.EducationType.NameEn;

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameEn)
            .Select(x => new EducationSubjectDto
            {
                Id = x.Id,
                EducationYearId = x.EducationYearId,
                EducationYearName = yearName,
                EducationStageId = year.EducationStageId,
                EducationStageName = stageName,
                EducationTypeId = year.EducationStage.EducationTypeId,
                EducationTypeName = typeName,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EducationSubjectDto>>.Success(items);
    }
}
