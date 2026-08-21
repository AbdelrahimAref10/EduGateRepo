using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationStage;

public sealed class CreateEducationStageCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateEducationStageCommand, Result<EducationStageDto>>
{
    public async Task<Result<EducationStageDto>> Handle(
        CreateEducationStageCommand request,
        CancellationToken cancellationToken)
    {
        var type = await dbContext.EducationTypes
            .FirstOrDefaultAsync(x => x.Id == request.EducationTypeId, cancellationToken);

        if (type is null)
            return Result<EducationStageDto>.NotFound("Education type was not found.");

        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.EducationStages
            .AnyAsync(
                x => x.EducationTypeId == request.EducationTypeId && x.NameEn == nameEn,
                cancellationToken);

        if (exists)
            return Result<EducationStageDto>.Conflict("Education stage already exists for this type.");

        var entity = new EducationStage
        {
            EducationTypeId = request.EducationTypeId,
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        dbContext.EducationStages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        entity.EducationType = type;
        return Result<EducationStageDto>.Success(
            EducationMappings.ToStageDto(entity, requestLanguage.Current, yearsCount: 0));
    }
}
