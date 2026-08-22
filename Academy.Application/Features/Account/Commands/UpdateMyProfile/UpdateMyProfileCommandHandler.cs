using Academy.Application.Common.Models;
using Academy.Application.Contracts.Images;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Account.Dtos;
using Academy.Application.Features.Account.Queries.GetMyProfile;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Account.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage,
    IImageService images)
    : IRequestHandler<UpdateMyProfileCommand, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        UpdateMyProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .AsTracking()
            .Include(x => x.StudentProfile)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result<UserProfileDto>.NotFound("User was not found.");

        var area = await dbContext.Areas
            .Include(x => x.City)
                .ThenInclude(x => x.Governorate)
                    .ThenInclude(x => x.Country)
            .FirstOrDefaultAsync(x => x.Id == request.AreaId && x.IsActive, cancellationToken);

        if (area is null)
            return Result<UserProfileDto>.Failure("Selected area was not found or is inactive.");

        var email = request.Email.Trim();
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null && existing.Id != user.Id)
                return Result<UserProfileDto>.Conflict("Email is already registered.");

            user.Email = email;
            user.UserName = email;
            user.NormalizedEmail = userManager.NormalizeEmail(email);
            user.NormalizedUserName = userManager.NormalizeName(email);
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();
        user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        user.AreaId = request.AreaId;

        if (request.PhotoBase64 is not null)
        {
            if (string.IsNullOrWhiteSpace(request.PhotoBase64))
            {
                user.ProfilePhoto = null;
            }
            else
            {
                var photo = images.Normalize(request.PhotoBase64);
                if (!photo.IsSuccess)
                    return Result<UserProfileDto>.Failure(photo.Error, photo.StatusCode);

                user.ProfilePhoto = photo.Value;
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var error = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            return Result<UserProfileDto>.Failure(error);
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var passwordResult = await userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword!,
                request.NewPassword!);

            if (!passwordResult.Succeeded)
            {
                var error = string.Join(" ", passwordResult.Errors.Select(e => e.Description));
                return Result<UserProfileDto>.Failure(error);
            }
        }

        // Reload navigation for response mapping
        user.Area = area;

        var roles = await userManager.GetRolesAsync(user);
        return Result<UserProfileDto>.Success(
            GetMyProfileQueryHandler.Map(user, roles, requestLanguage.Current));
    }
}
