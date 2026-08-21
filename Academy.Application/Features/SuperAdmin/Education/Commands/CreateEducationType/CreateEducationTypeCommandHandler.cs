using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationType;

public sealed class CreateEducationTypeCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateEducationTypeCommand, Result<EducationTypeDto>>
{
    public async Task<Result<EducationTypeDto>> Handle(
        CreateEducationTypeCommand request,
        CancellationToken cancellationToken)
    {
        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.EducationTypes
            .AnyAsync(x => x.NameEn == nameEn, cancellationToken);

        if (exists)
            return Result<EducationTypeDto>.Conflict("Education type already exists.");

        var entity = new EducationType
        {
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        dbContext.EducationTypes.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EducationTypeDto>.Success(new EducationTypeDto
        {
            Id = entity.Id,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            YearsCount = 0
        });
    }
}
