using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetMyCityAreas;

public sealed class GetMyCityAreasQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetMyCityAreasQuery, Result<IReadOnlyList<AreaDto>>>
{
    public async Task<Result<IReadOnlyList<AreaDto>>> Handle(
        GetMyCityAreasQuery request,
        CancellationToken cancellationToken)
    {
        var cityId = await dbContext.Teachers
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.User.Area!.CityId)
            .FirstOrDefaultAsync(cancellationToken);

        if (cityId is null)
            return Result<IReadOnlyList<AreaDto>>.NotFound("Teacher city was not found.");

        var language = requestLanguage.Current;

        var areas = await dbContext.Areas
            .Where(x => x.CityId == cityId.Value && x.IsActive)
            .OrderBy(x => x.NameEn)
            .Select(x => new AreaDto
            {
                Id = x.Id,
                CityId = x.CityId,
                Name = language == AppLanguage.Arabic ? x.NameAr : x.NameEn,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AreaDto>>.Success(areas);
    }
}
