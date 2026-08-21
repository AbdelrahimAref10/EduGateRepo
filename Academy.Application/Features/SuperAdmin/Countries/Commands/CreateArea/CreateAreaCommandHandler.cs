using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateArea;

public sealed class CreateAreaCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<CreateAreaCommand, Result<AreaDto>>
{
    public async Task<Result<AreaDto>> Handle(
        CreateAreaCommand request,
        CancellationToken cancellationToken)
    {
        var cityExists = await dbContext.Cities
            .AnyAsync(x => x.Id == request.CityId && x.IsActive, cancellationToken);

        if (!cityExists)
            return Result<AreaDto>.NotFound("City was not found.");

        var nameEn = request.NameEn.Trim();

        var exists = await dbContext.Areas
            .AnyAsync(
                x => x.CityId == request.CityId && x.NameEn == nameEn,
                cancellationToken);

        if (exists)
            return Result<AreaDto>.Conflict("Area already exists for this city.");

        var entity = new Area
        {
            CityId = request.CityId,
            NameAr = request.NameAr.Trim(),
            NameEn = nameEn,
            IsActive = true
        };

        dbContext.Areas.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AreaDto>.Success(new AreaDto
        {
            Id = entity.Id,
            CityId = entity.CityId,
            Name = LocalizedNames.Pick(entity.NameAr, entity.NameEn, requestLanguage.Current),
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            IsActive = entity.IsActive
        });
    }
}
