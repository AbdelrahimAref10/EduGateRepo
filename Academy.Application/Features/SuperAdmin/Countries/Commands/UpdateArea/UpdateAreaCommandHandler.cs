using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateArea;

public sealed class UpdateAreaCommandHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<UpdateAreaCommand, Result<AreaDto>>
{
    public async Task<Result<AreaDto>> Handle(
        UpdateAreaCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Areas
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<AreaDto>.NotFound("Area was not found.");

        var nameEn = request.NameEn.Trim();

        var nameTaken = await dbContext.Areas
            .AnyAsync(
                x => x.Id != request.Id && x.CityId == entity.CityId && x.NameEn == nameEn,
                cancellationToken);

        if (nameTaken)
            return Result<AreaDto>.Conflict("Area already exists for this city.");

        entity.NameAr = request.NameAr.Trim();
        entity.NameEn = nameEn;

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
