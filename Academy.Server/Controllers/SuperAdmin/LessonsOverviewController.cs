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
}
