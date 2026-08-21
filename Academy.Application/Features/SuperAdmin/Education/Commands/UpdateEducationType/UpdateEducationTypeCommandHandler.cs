using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationType;

public sealed class UpdateEducationTypeCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateEducationTypeCommand, Result<EducationTypeDto>>
{
    public async Task<Result<EducationTypeDto>> Handle(
        UpdateEducationTypeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.EducationTypes
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<EducationTypeDto>.NotFound("Education type was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.EducationTypes
            .AnyAsync(x => x.Id != request.Id && x.NameEn == nameEn, cancellationToken);

        if (nameTaken)
            return Result<EducationTypeDto>.Conflict("Education type already exists.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;
        entity.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync(cancellationToken);

        var stagesCount = await dbContext.EducationStages
            .CountAsync(x => x.EducationTypeId == entity.Id, cancellationToken);

        return Result<EducationTypeDto>.Success(
            EducationMappings.ToTypeDto(entity, requestLanguage.Current, stagesCount));
    }
}
