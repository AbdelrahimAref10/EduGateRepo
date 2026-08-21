using Academy.Application.Common.Localization;
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
        var typeExists = await dbContext.EducationTypes
            .AnyAsync(x => x.Id == request.EducationTypeId, cancellationToken);

        if (!typeExists)
            return Result<EducationYearDto>.NotFound("Education type was not found.");

        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.EducationYears
            .AnyAsync(
                x => x.EducationTypeId == request.EducationTypeId && x.NameEn == nameEn,
                cancellationToken);

        if (exists)
            return Result<EducationYearDto>.Conflict("Education year already exists for this type.");

        var entity = new EducationYear
        {
            EducationTypeId = request.EducationTypeId,
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        dbContext.EducationYears.Add(entity);
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
