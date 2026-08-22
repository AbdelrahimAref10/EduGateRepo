using System.Security.Claims;
using Academy.Application.Features.Marketplace.Commands.CreateTeacherReview;
using Academy.Application.Features.Marketplace.Commands.UpdateTeacherReview;
using Academy.Application.Features.Marketplace.Dtos;
using Academy.Application.Features.Marketplace.Queries.GetMyTeacherReview;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Student;

[ApiController]
[Authorize(Roles = AppRoles.Student)]
[Route("api/student/teachers")]
[Produces("application/json")]
public sealed class StudentTeacherReviewsController(ISender sender) : ControllerBase
{
    [HttpGet("{teacherId:int}/reviews/mine")]
    [ProducesResponseType(typeof(MyTeacherReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMine(int teacherId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyTeacherReviewQuery(userId.Value, teacherId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{teacherId:int}/reviews")]
    [ProducesResponseType(typeof(TeacherReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        int teacherId,
        [FromBody] UpsertTeacherReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new CreateTeacherReviewCommand(userId.Value, teacherId, request.Rating, request.Comment),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{teacherId:int}/reviews")]
    [ProducesResponseType(typeof(TeacherReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int teacherId,
        [FromBody] UpsertTeacherReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateTeacherReviewCommand(userId.Value, teacherId, request.Rating, request.Comment),
            cancellationToken);

        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
