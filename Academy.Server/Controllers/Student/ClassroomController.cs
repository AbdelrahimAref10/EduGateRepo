using System.Security.Claims;
using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Classroom.Commands.StartStudentSessionExam;
using Academy.Application.Features.Student.Classroom.Commands.SubmitStudentSessionExam;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Application.Features.Student.Classroom.Queries.GetMyStudentClassrooms;
using Academy.Application.Features.Student.Classroom.Queries.GetMyStudentExams;
using Academy.Application.Features.Student.Classroom.Queries.GetStudentClassroom;
using Academy.Application.Features.Student.Classroom.Queries.GetStudentClassroomMaterialFile;
using Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Student;

[ApiController]
[Authorize(Roles = AppRoles.Student)]
[Route("api/student/classroom")]
[Produces("application/json")]
public sealed class ClassroomController(ISender sender) : ControllerBase
{
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentClassroomSessionListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyClassrooms(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(new GetMyStudentClassroomsQuery(userId.Value), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("exams")]
    [ProducesResponseType(typeof(PagedResult<StudentExamListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyExams(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetMyStudentExamsQuery(userId.Value, page, pageSize),
            cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("sessions/{sessionId:int}")]
    [ProducesResponseType(typeof(StudentClassroomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetClassroom(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetStudentClassroomQuery(userId.Value, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("sessions/{sessionId:int}/materials/{materialId:int}/file")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DownloadMaterialFile(
        int sessionId,
        int materialId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetStudentClassroomMaterialFileQuery(userId.Value, sessionId, materialId),
            cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var file = result.Value!;
        return File(file.Stream, file.ContentType, file.FileName);
    }

    [HttpGet("sessions/{sessionId:int}/exam")]
    [ProducesResponseType(typeof(StudentExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetExam(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetStudentSessionExamQuery(userId.Value, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("sessions/{sessionId:int}/exam/start")]
    [ProducesResponseType(typeof(StudentExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartExam(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new StartStudentSessionExamCommand(userId.Value, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("sessions/{sessionId:int}/exam/answer")]
    [ProducesResponseType(typeof(StudentExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AnswerExamQuestion(
        int sessionId,
        [FromBody] AnswerStudentExamQuestionRequest? request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new SubmitStudentSessionExamCommand(
                userId.Value,
                sessionId,
                request?.OptionId),
            cancellationToken);

        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
