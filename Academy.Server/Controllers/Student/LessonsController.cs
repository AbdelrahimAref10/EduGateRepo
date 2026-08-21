using System.Security.Claims;
using Academy.Application.Features.Student.Lessons.Commands.BookLesson;
using Academy.Application.Features.Student.Lessons.Dtos;
using Academy.Application.Features.Student.Lessons.Queries.GetAvailableLessons;
using Academy.Application.Features.Student.Lessons.Queries.GetMyStudentLessons;
using Academy.Application.Features.Student.Lessons.Queries.GetStudentLessonDetail;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Student;

[ApiController]
[Authorize(Roles = AppRoles.Student)]
[Route("api/student/lessons")]
[Produces("application/json")]
public sealed class LessonsController(ISender sender) : ControllerBase
{
    /// <summary>Catalog of lessons the student can still book.</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyList<AvailableLessonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetAvailableLessonsAlias(CancellationToken cancellationToken)
        => GetAvailableLessons(cancellationToken);

    /// <summary>Backward-compatible alias of <see cref="GetAvailableLessonsAlias"/>.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AvailableLessonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableLessons(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetAvailableLessonsQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Lessons the student has booked (Pending / Confirmed / Rejected).</summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentLessonListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyLessons(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyStudentLessonsQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Lesson detail + assigned group sessions (if any).</summary>
    [HttpGet("{lessonId:int}")]
    [ProducesResponseType(typeof(StudentLessonDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyLessonDetail(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetStudentLessonDetailQuery(userId.Value, lessonId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/book")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookLesson(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new BookLessonCommand(userId.Value, lessonId), cancellationToken);
        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
