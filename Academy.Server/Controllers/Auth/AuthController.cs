using Academy.Application.Features.Auth.Commands.Login;
using Academy.Application.Features.Auth.Commands.RefreshToken;
using Academy.Application.Features.Auth.Commands.Register;
using Academy.Application.Features.Auth.Dtos;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Auth;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Role,
                request.AreaId),
            cancellationToken);

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return result.ToActionResult();
    }
}
