using System.Security.Claims;
using Academy.Application.Features.Reviews.Commands.UpsertLessonReview;
using Academy.Application.Features.Reviews.Commands.UpsertSessionReview;
using Academy.Application.Features.Reviews.Dtos;
using Academy.Application.Features.Reviews.Queries.GetMyLessonReview;
using Academy.Application.Features.Reviews.Queries.GetMySessionReview;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Student;

[ApiController]
[Authorize(Roles = AppRoles.Student)]
[Route("api/student")]
[Produces("application/json")]
public sealed class StudentReviewsController(ISender sender) : ControllerBase
{
    [HttpGet("lessons/{lessonId:int}/reviews/mine")]
    [ProducesResponseType(typeof(MyTargetReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyLessonReview(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyLessonReviewQuery(userId.Value, lessonId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("lessons/{lessonId:int}/reviews")]
    [ProducesResponseType(typeof(TargetReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertLessonReview(
        int lessonId,
        [FromBody] UpsertReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpsertLessonReviewCommand(userId.Value, lessonId, request.Rating, request.Comment),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("sessions/{sessionId:int}/reviews/mine")]
    [ProducesResponseType(typeof(MyTargetReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMySessionReview(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMySessionReviewQuery(userId.Value, sessionId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("sessions/{sessionId:int}/reviews")]
    [ProducesResponseType(typeof(TargetReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertSessionReview(
        int sessionId,
        [FromBody] UpsertReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpsertSessionReviewCommand(userId.Value, sessionId, request.Rating, request.Comment),
            cancellationToken);
        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
