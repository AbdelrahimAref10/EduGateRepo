using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationYear;

public sealed class UpdateEducationYearCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateEducationYearCommand, Result<EducationYearDto>>
{
    public async Task<Result<EducationYearDto>> Handle(
        UpdateEducationYearCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationYears
            .AsTracking()
            .Include(x => x.EducationStage)
                .ThenInclude(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == request.YearId
                    && x.EducationStageId == request.EducationStageId
                    && x.EducationStage.EducationTypeId == request.EducationTypeId,
                cancellationToken);

        if (entity is null)
            return Result<EducationYearDto>.NotFound("Education year was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.EducationYears
            .AnyAsync(
                x => x.Id != request.YearId
                    && x.EducationStageId == request.EducationStageId
                    && x.NameEn == nameEn,
                cancellationToken);

        if (nameTaken)
            return Result<EducationYearDto>.Conflict("Education year already exists for this stage.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;
        entity.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync(cancellationToken);

        var subjectsCount = await dbContext.EducationSubjects
            .CountAsync(x => x.EducationYearId == entity.Id, cancellationToken);

        return Result<EducationYearDto>.Success(
            EducationMappings.ToYearDto(entity, requestLanguage.Current, subjectsCount));
    }
}
