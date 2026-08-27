using System.Security.Claims;
using Academy.Application.Features.Marketplace.Dtos;
using Academy.Application.Features.Marketplace.Queries.GetPublicHighlights;
using Academy.Application.Features.Marketplace.Queries.GetPublicLesson;
using Academy.Application.Features.Marketplace.Queries.GetPublicTeacher;
using Academy.Application.Features.Marketplace.Queries.GetPublicTeachers;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
[Produces("application/json")]
public sealed class PublicMarketplaceController(ISender sender) : ControllerBase
{
    [HttpGet("highlights")]
    [ProducesResponseType(typeof(PublicHighlightsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHighlights(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicHighlightsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("teachers")]
    [ProducesResponseType(typeof(IReadOnlyList<PublicTeacherListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeachers(
        [FromQuery] int? countryId,
        [FromQuery] int? academicYearId,
        [FromQuery] int? educationStageId,
        [FromQuery] int? educationSubjectId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetPublicTeachersQuery(countryId, academicYearId, educationStageId, educationSubjectId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("teachers/{teacherId:int}")]
    [ProducesResponseType(typeof(PublicTeacherDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeacher(int teacherId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetPublicTeacherQuery(teacherId, GetUserId()),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}")]
    [ProducesResponseType(typeof(PublicLessonDeepLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLesson(int lessonId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicLessonQuery(lessonId), cancellationToken);
        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
