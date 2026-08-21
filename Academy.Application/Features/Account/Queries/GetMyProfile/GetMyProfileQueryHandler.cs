using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Features.Account.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Account.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler(
    UserManager<ApplicationUser> userManager,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetMyProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .Include(x => x.Area!)
                .ThenInclude(x => x.City)
                    .ThenInclude(x => x.Governorate)
                        .ThenInclude(x => x.Country)
            .Include(x => x.StudentProfile)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result<UserProfileDto>.NotFound("User was not found.");

        var roles = await userManager.GetRolesAsync(user);

        return Result<UserProfileDto>.Success(Map(user, roles, requestLanguage.Current));
    }

    internal static UserProfileDto Map(
        ApplicationUser user,
        IList<string> roles,
        AppLanguage language)
    {
        var area = user.Area;
        var city = area?.City;
        var governorate = city?.Governorate;
        var country = governorate?.Country;

        return new UserProfileDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            Roles = roles.ToList(),
            LanguageId = (int)user.PreferredLanguage,
            StudentCode = user.StudentProfile?.StudentCode,
            IsParent = user.StudentProfile?.IsParent,
            AreaId = user.AreaId,
            AreaName = LocalizedNames.PickOptional(area?.NameAr, area?.NameEn, language),
            CityId = city?.Id,
            CityName = LocalizedNames.PickOptional(city?.NameAr, city?.NameEn, language),
            GovernorateId = governorate?.Id,
            GovernorateName = LocalizedNames.PickOptional(governorate?.NameAr, governorate?.NameEn, language),
            CountryId = country?.Id,
            CountryName = LocalizedNames.PickOptional(country?.NameAr, country?.NameEn, language)
        };
    }
}
