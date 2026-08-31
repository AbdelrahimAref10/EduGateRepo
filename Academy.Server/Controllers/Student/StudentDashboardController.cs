using System.Security.Claims;
using Academy.Application.Features.LearningPath.Dtos;
using Academy.Application.Features.Student.Learning.Queries.GetStudentProgress;
using Academy.Application.Features.Student.Learning.Queries.GetStudentWeeklyPlan;
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
public sealed class StudentDashboardController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(WeeklyLearningPlanDto), StatusCodes.Status200OK)]
    public Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        => GetWeeklyPlan(cancellationToken);

    [HttpGet("learning/plan")]
    [ProducesResponseType(typeof(WeeklyLearningPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyPlan(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetStudentWeeklyPlanQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("learning/progress")]
    [ProducesResponseType(typeof(ProgressReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgress(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetStudentProgressQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
