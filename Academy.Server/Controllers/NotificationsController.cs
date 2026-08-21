using System.Security.Claims;
using Academy.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using Academy.Application.Features.Notifications.Commands.MarkNotificationRead;
using Academy.Application.Features.Notifications.Dtos;
using Academy.Application.Features.Notifications.Queries.GetMyNotifications;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
[Produces("application/json")]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyNotificationsQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{notificationId:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(int notificationId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new MarkNotificationReadCommand(userId.Value, notificationId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new MarkAllNotificationsReadCommand(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
