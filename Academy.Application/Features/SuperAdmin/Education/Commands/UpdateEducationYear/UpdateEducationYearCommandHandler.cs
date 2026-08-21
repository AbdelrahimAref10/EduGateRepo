using Academy.Application.Common.Localization;
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
            .FirstOrDefaultAsync(
                x => x.Id == request.YearId && x.EducationTypeId == request.EducationTypeId,
                cancellationToken);

        if (entity is null)
            return Result<EducationYearDto>.NotFound("Education year was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.EducationYears
            .AnyAsync(
                x => x.Id != request.YearId
                    && x.EducationTypeId == request.EducationTypeId
                    && x.NameEn == nameEn,
                cancellationToken);

        if (nameTaken)
            return Result<EducationYearDto>.Conflict("Education year already exists for this type.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;
        entity.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EducationYearDto>.Success(new EducationYearDto
        {
            Id = entity.Id,
            EducationTypeId = entity.EducationTypeId,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        });
    }
}
