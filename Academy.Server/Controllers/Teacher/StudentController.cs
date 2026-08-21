using System.Security.Claims;
using Academy.Application.Features.Teacher.StudentBookings.Commands.ConfirmBooking;
using Academy.Application.Features.Teacher.StudentBookings.Commands.RejectBooking;
using Academy.Application.Features.Teacher.StudentBookings.Dtos;
using Academy.Application.Features.Teacher.StudentBookings.Queries.GetAllBookings;
using Academy.Application.Features.Teacher.StudentBookings.Queries.GetPendingBookings;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Teacher;

[ApiController]
[Authorize(Roles = AppRoles.Teacher)]
[Route("api/teacher/student")]
[Produces("application/json")]
public sealed class StudentController(ISender sender) : ControllerBase
{
    [HttpGet("bookings/pending")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherBookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPendingBookings(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetPendingBookingsQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("bookings")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherBookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllBookings(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetAllBookingsQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("bookings/{bookingId:int}/confirm")]
    [ProducesResponseType(typeof(TeacherBookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmBooking(int bookingId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new ConfirmBookingCommand(userId.Value, bookingId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("bookings/{bookingId:int}/reject")]
    [ProducesResponseType(typeof(TeacherBookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectBooking(int bookingId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new RejectBookingCommand(userId.Value, bookingId), cancellationToken);
        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
