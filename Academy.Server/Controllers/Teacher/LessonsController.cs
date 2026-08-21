using System.Security.Claims;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Application.Features.Teacher.Lessons.Commands.AddGroupMemberByCode;
using Academy.Application.Features.Teacher.Lessons.Commands.AddLessonStudentByCode;
using Academy.Application.Features.Teacher.Lessons.Commands.CreateLesson;
using Academy.Application.Features.Teacher.Lessons.Commands.CreateLessonGroup;
using Academy.Application.Features.Teacher.Lessons.Commands.DeleteLesson;
using Academy.Application.Features.Teacher.Lessons.Commands.DeleteLessonGroup;
using Academy.Application.Features.Teacher.Lessons.Commands.EndLessonGroup;
using Academy.Application.Features.Teacher.Lessons.Commands.EndLessonGroupSession;
using Academy.Application.Features.Teacher.Lessons.Commands.RemoveGroupMember;
using Academy.Application.Features.Teacher.Lessons.Commands.StartLesson;
using Academy.Application.Features.Teacher.Lessons.Commands.StartLessonGroup;
using Academy.Application.Features.Teacher.Lessons.Commands.StartLessonGroupSession;
using Academy.Application.Features.Teacher.Lessons.Commands.UpdateLesson;
using Academy.Application.Features.Teacher.Lessons.Commands.UpdateLessonGroup;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroup;
using Academy.Application.Features.Teacher.Lessons.Queries.GetLessonManage;
using Academy.Application.Features.Teacher.Lessons.Queries.GetMyCityAreas;
using Academy.Application.Features.Teacher.Lessons.Queries.GetMyLessons;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Teacher;

[ApiController]
[Authorize(Roles = AppRoles.Teacher)]
[Route("api/teacher/lessons")]
[Produces("application/json")]
public sealed class LessonsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LessonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLessons(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyLessonsQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("my-city-areas")]
    [ProducesResponseType(typeof(IReadOnlyList<AreaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyCityAreas(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyCityAreasQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{lessonId:int}")]
    [ProducesResponseType(typeof(LessonManageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonManage(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetLessonManageQuery(userId.Value, lessonId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLesson(
        [FromBody] CreateLessonRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new CreateLessonCommand(
                userId.Value,
                request.EducationTypeId,
                request.EducationStageId,
                request.EducationYearId,
                request.EducationSubjectId,
                request.BillingType,
                request.SessionPrice,
                request.MonthlyPrice,
                request.StartDate,
                request.AreaId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{lessonId:int}")]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateLesson(
        int lessonId,
        [FromBody] UpdateLessonRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateLessonCommand(
                userId.Value,
                lessonId,
                request.EducationTypeId,
                request.EducationStageId,
                request.EducationYearId,
                request.EducationSubjectId,
                request.BillingType,
                request.SessionPrice,
                request.MonthlyPrice,
                request.StartDate,
                request.AreaId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{lessonId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteLesson(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new DeleteLessonCommand(userId.Value, lessonId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/start")]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartLesson(int lessonId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new StartLessonCommand(userId.Value, lessonId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/students")]
    [ProducesResponseType(typeof(LessonStudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddLessonStudent(
        int lessonId,
        [FromBody] AddLessonStudentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new AddLessonStudentByCodeCommand(userId.Value, lessonId, request.StudentCode),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/groups")]
    [ProducesResponseType(typeof(LessonGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateGroup(
        int lessonId,
        [FromBody] CreateLessonGroupRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new CreateLessonGroupCommand(
                userId.Value,
                lessonId,
                request.Name,
                request.Dates,
                request.PeriodStartDate,
                request.PeriodEndDate,
                request.AreaId,
                request.Address,
                request.Notes,
                request.MaxCapacity),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{lessonId:int}/groups/{groupId:int}")]
    [ProducesResponseType(typeof(LessonGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroup(
        int lessonId,
        int groupId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetLessonGroupQuery(userId.Value, lessonId, groupId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{lessonId:int}/groups/{groupId:int}")]
    [ProducesResponseType(typeof(LessonGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateGroup(
        int lessonId,
        int groupId,
        [FromBody] UpdateLessonGroupRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateLessonGroupCommand(
                userId.Value,
                lessonId,
                groupId,
                request.Name,
                request.Dates,
                request.PeriodStartDate,
                request.PeriodEndDate,
                request.AreaId,
                request.Address,
                request.Notes,
                request.MaxCapacity),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{lessonId:int}/groups/{groupId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteGroup(
        int lessonId,
        int groupId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new DeleteLessonGroupCommand(userId.Value, lessonId, groupId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/groups/{groupId:int}/end")]
    [ProducesResponseType(typeof(LessonGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndGroup(
        int lessonId,
        int groupId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new EndLessonGroupCommand(userId.Value, lessonId, groupId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/groups/{groupId:int}/start")]
    [ProducesResponseType(typeof(LessonGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartGroup(
        int lessonId,
        int groupId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new StartLessonGroupCommand(userId.Value, lessonId, groupId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/groups/{groupId:int}/sessions/{sessionId:int}/start")]
    [ProducesResponseType(typeof(LessonGroupSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartSession(
        int lessonId,
        int groupId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new StartLessonGroupSessionCommand(userId.Value, lessonId, groupId, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/groups/{groupId:int}/sessions/{sessionId:int}/end")]
    [ProducesResponseType(typeof(LessonGroupSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EndSession(
        int lessonId,
        int groupId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new EndLessonGroupSessionCommand(userId.Value, lessonId, groupId, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{lessonId:int}/groups/{groupId:int}/members")]
    [ProducesResponseType(typeof(LessonGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddGroupMember(
        int lessonId,
        int groupId,
        [FromBody] AddGroupMemberRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new AddGroupMemberByCodeCommand(
                userId.Value,
                lessonId,
                groupId,
                request.StudentId,
                request.StudentCode),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{lessonId:int}/groups/{groupId:int}/members/{studentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveGroupMember(
        int lessonId,
        int groupId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new RemoveGroupMemberCommand(userId.Value, lessonId, groupId, studentId),
            cancellationToken);

        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
