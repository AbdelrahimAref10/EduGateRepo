using System.Security.Claims;
using Academy.Application.Common.Models;
using Academy.Application.Features.Parent.Commands.LinkChild;
using Academy.Application.Features.Parent.Commands.UnlinkChild;
using Academy.Application.Features.Parent.Dtos;
using Academy.Application.Features.Parent.Queries.GetMyChildren;
using Academy.Application.Features.Parent.Queries.GetParentAttendance;
using Academy.Application.Features.Parent.Queries.GetParentChildExam;
using Academy.Application.Features.Parent.Queries.GetParentChildExams;
using Academy.Application.Features.Parent.Queries.GetParentDashboard;
using Academy.Application.Features.Parent.Queries.GetParentPayments;
using Academy.Application.Features.Parent.Queries.GetParentProgress;
using Academy.Application.Features.Parent.Queries.GetParentWeeklyPlan;
using Academy.Application.Features.LearningPath.Dtos;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Parent;

[ApiController]
[Authorize(Roles = AppRoles.Parent)]
[Route("api/parent")]
[Produces("application/json")]
public sealed class ParentController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ParentDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetParentDashboardQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("children")]
    [ProducesResponseType(typeof(IReadOnlyList<ParentChildDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildren(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyChildrenQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("children/link")]
    [ProducesResponseType(typeof(ParentChildDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> LinkChild(
        [FromBody] LinkChildRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new LinkChildCommand(userId.Value, request.StudentCode),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("children/{childStudentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UnlinkChild(int childStudentId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UnlinkChildCommand(userId.Value, childStudentId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("learning/plan")]
    [ProducesResponseType(typeof(WeeklyLearningPlanDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyPlan(
        [FromQuery] int? childStudentId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetParentWeeklyPlanQuery(userId.Value, childStudentId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("learning/progress")]
    [ProducesResponseType(typeof(ProgressReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgress(
        [FromQuery] int? childStudentId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetParentProgressQuery(userId.Value, childStudentId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("exams")]
    [ProducesResponseType(typeof(PagedResult<ParentExamListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExams(
        [FromQuery] int? childStudentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetParentChildExamsQuery(userId.Value, childStudentId, page, pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("children/{childStudentId:int}/sessions/{sessionId:int}/exam")]
    [ProducesResponseType(typeof(StudentExamDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildExam(
        int childStudentId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetParentChildExamQuery(userId.Value, childStudentId, sessionId),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("attendance")]
    [ProducesResponseType(typeof(PagedResult<ParentAttendanceItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] int? childStudentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetParentAttendanceQuery(userId.Value, childStudentId, page, pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("payments")]
    [ProducesResponseType(typeof(PagedResult<ParentPaymentItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int? childStudentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetParentPaymentsQuery(userId.Value, childStudentId, page, pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
