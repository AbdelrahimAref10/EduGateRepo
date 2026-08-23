using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminGroupSessions;
using Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminLessonReviews;
using Academy.Application.Features.SuperAdmin.Lessons.Dtos;
using Academy.Application.Features.SuperAdmin.Lessons.Queries.GetAllGroups;
using Academy.Application.Features.SuperAdmin.Lessons.Queries.GetAllLessons;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Authorize(Roles = AppRoles.SuperAdmin)]
[Route("api/super-admin")]
[Produces("application/json")]
public sealed class LessonsOverviewController(ISender sender) : ControllerBase
{
    [HttpGet("lessons")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminLessonListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLessons(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllLessonsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("groups")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminGroupListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllGroups(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGroupsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("groups/{groupId:int}/sessions")]
    [ProducesResponseType(typeof(AdminGroupSessionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroupSessions(int groupId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminGroupSessionsQuery(groupId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("lessons/{lessonId:int}/reviews")]
    [ProducesResponseType(typeof(AdminReviewsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonReviews(int lessonId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminLessonReviewsQuery(lessonId), cancellationToken);
        return result.ToActionResult();
    }
}
