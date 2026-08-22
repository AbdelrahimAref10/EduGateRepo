using System.Security.Claims;
using Academy.Application.Features.Account.Commands.UpdateMyProfile;
using Academy.Application.Features.Account.Commands.UpdatePreferredLanguage;
using Academy.Application.Features.Account.Dtos;
using Academy.Application.Features.Account.Queries.GetMyProfile;
using Academy.Application.Features.Auth.Dtos;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Account;

[ApiController]
[Authorize]
[Route("api/account")]
[Produces("application/json")]
public sealed class AccountController(ISender sender) : ControllerBase
{
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyProfileQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("profile")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateMyProfileCommand(
                userId.Value,
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Bio,
                request.PhotoBase64,
                request.AreaId,
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmNewPassword),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("language")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreferredLanguage(
        [FromBody] UpdatePreferredLanguageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdatePreferredLanguageCommand(userId.Value, request.LanguageId),
            cancellationToken);

        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
