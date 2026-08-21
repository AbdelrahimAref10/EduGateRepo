using Academy.Application.Common.Models;
using Academy.Application.Features.Account.Dtos;
using MediatR;

namespace Academy.Application.Features.Account.Commands.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Bio,
    int AreaId,
    string? CurrentPassword,
    string? NewPassword,
    string? ConfirmNewPassword) : IRequest<Result<UserProfileDto>>;
