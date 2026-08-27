using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationYear;

public sealed class CreateEducationYearCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateEducationYearCommand, Result<EducationYearDto>>
{
    public async Task<Result<EducationYearDto>> Handle(
        CreateEducationYearCommand request,
        CancellationToken cancellationToken)
    {
        var stage = await dbContext.EducationStages
            .FirstOrDefaultAsync(x => x.Id == request.EducationStageId, cancellationToken);

        if (stage is null)
            return Result<EducationYearDto>.NotFound("Education stage was not found.");

        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.EducationYears
            .AnyAsync(
                x => x.EducationStageId == request.EducationStageId && x.NameEn == nameEn,
                cancellationToken);

        if (exists)
            return Result<EducationYearDto>.Conflict("Education year already exists for this stage.");

        var entity = new EducationYear
        {
            EducationStageId = request.EducationStageId,
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        dbContext.EducationYears.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        entity.EducationStage = stage;
        return Result<EducationYearDto>.Success(
            EducationMappings.ToYearDto(entity, requestLanguage.Current, subjectsCount: 0));
    }
}
