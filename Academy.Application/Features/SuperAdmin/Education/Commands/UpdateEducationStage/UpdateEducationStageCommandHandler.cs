using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationStage;

public sealed class UpdateEducationStageCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateEducationStageCommand, Result<EducationStageDto>>
{
    public async Task<Result<EducationStageDto>> Handle(
        UpdateEducationStageCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationStages
            .AsTracking()
            .Include(x => x.EducationType)
            .FirstOrDefaultAsync(
                x => x.Id == request.StageId && x.EducationTypeId == request.EducationTypeId,
                cancellationToken);

        if (entity is null)
            return Result<EducationStageDto>.NotFound("Education stage was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.EducationStages
            .AnyAsync(
                x => x.Id != request.StageId
                    && x.EducationTypeId == request.EducationTypeId
                    && x.NameEn == nameEn,
                cancellationToken);

        if (nameTaken)
            return Result<EducationStageDto>.Conflict("Education stage already exists for this type.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;
        entity.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync(cancellationToken);

        var yearsCount = await dbContext.EducationYears
            .CountAsync(x => x.EducationStageId == entity.Id, cancellationToken);

        return Result<EducationStageDto>.Success(
            EducationMappings.ToStageDto(entity, requestLanguage.Current, yearsCount));
    }
}
