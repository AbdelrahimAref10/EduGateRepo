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
        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.EducationStages
            .AnyAsync(x => x.NameEn == nameEn, cancellationToken);

        if (exists)
            return Result<EducationStageDto>.Conflict("Education stage already exists.");

        var entity = new EducationStage
        {
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        dbContext.EducationStages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EducationStageDto>.Success(
            EducationMappings.ToStageDto(entity, requestLanguage.Current, yearsCount: 0));
    }
}
