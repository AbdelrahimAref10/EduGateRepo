using System.Security.Claims;
using Academy.Application.Contracts.Ai;
using Academy.Application.Features.Teacher.Classroom.Commands.CreateClassroomMaterial;
using Academy.Application.Features.Teacher.Classroom.Commands.DeleteClassroomMaterial;
using Academy.Application.Features.Teacher.Classroom.Commands.GenerateSessionExam;
using Academy.Application.Features.Teacher.Classroom.Commands.PublishSessionExam;
using Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomInfo;
using Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomMaterial;
using Academy.Application.Features.Teacher.Classroom.Commands.UpdateStudentSessionDetail;
using Academy.Application.Features.Teacher.Classroom.Commands.UploadClassroomMaterial;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherClassroom;
using Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherClassroomMaterialFile;
using Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherSessionExam;
using Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherSessionExamResults;
using Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherStudentExamReview;
using Academy.Application.Features.Classroom.Exams;
using Academy.Domain.Common;
using Academy.Domain.Enums;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.Teacher;

[ApiController]
[Authorize(Roles = AppRoles.Teacher)]
[Route("api/teacher/classroom")]
[Produces("application/json")]
public sealed class ClassroomController(ISender sender) : ControllerBase
{
    [HttpGet("sessions/{sessionId:int}")]
    [ProducesResponseType(typeof(TeacherClassroomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassroom(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherClassroomQuery(userId.Value, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("sessions/{sessionId:int}")]
    [ProducesResponseType(typeof(TeacherClassroomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClassroomInfo(
        int sessionId,
        [FromBody] UpdateClassroomInfoRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateClassroomInfoCommand(
                userId.Value,
                sessionId,
                request.Topic,
                request.Description),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("sessions/{sessionId:int}/students/{studentId:int}")]
    [ProducesResponseType(typeof(ClassroomStudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudentDetail(
        int sessionId,
        int studentId,
        [FromBody] UpdateStudentSessionDetailRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateStudentSessionDetailCommand(
                userId.Value,
                sessionId,
                studentId,
                request.IsPresent,
                request.IsPaid,
                request.TeacherNotes),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("sessions/{sessionId:int}/materials")]
    [ProducesResponseType(typeof(ClassroomMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMaterial(
        int sessionId,
        [FromBody] CreateClassroomMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new CreateClassroomMaterialCommand(
                userId.Value,
                sessionId,
                request.Title,
                request.Description,
                request.MaterialType,
                request.ExternalUrl,
                request.Body,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("sessions/{sessionId:int}/materials/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ClassroomMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadMaterial(
        int sessionId,
        [FromForm] IFormFile file,
        [FromForm] string? description,
        [FromForm] string? title,
        [FromForm] int? sortOrder,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Title = "Bad Request",
                Detail = "A file is required.",
                Status = StatusCodes.Status400BadRequest
            });

        await using var stream = file.OpenReadStream();

        var resolvedTitle = string.IsNullOrWhiteSpace(title)
            ? Path.GetFileNameWithoutExtension(file.FileName)
            : title;

        var result = await sender.Send(
            new UploadClassroomMaterialCommand(
                userId.Value,
                sessionId,
                resolvedTitle ?? "material",
                description,
                ClassroomMaterialType.File,
                stream,
                file.FileName,
                file.ContentType ?? "application/octet-stream",
                sortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("sessions/{sessionId:int}/materials/{materialId:int}")]
    [ProducesResponseType(typeof(ClassroomMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMaterial(
        int sessionId,
        int materialId,
        [FromBody] UpdateClassroomMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new UpdateClassroomMaterialCommand(
                userId.Value,
                sessionId,
                materialId,
                request.Title,
                request.Description,
                request.MaterialType,
                request.ExternalUrl,
                request.Body,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("sessions/{sessionId:int}/materials/{materialId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMaterial(
        int sessionId,
        int materialId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new DeleteClassroomMaterialCommand(userId.Value, sessionId, materialId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("sessions/{sessionId:int}/materials/{materialId:int}/file")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadMaterialFile(
        int sessionId,
        int materialId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherClassroomMaterialFileQuery(userId.Value, sessionId, materialId),
            cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var file = result.Value!;
        return File(file.Stream, file.ContentType, file.FileName);
    }

    [HttpGet("sessions/{sessionId:int}/exam")]
    [ProducesResponseType(typeof(TeacherExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExam(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherSessionExamQuery(userId.Value, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("sessions/{sessionId:int}/exam")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(80_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 80_000_000)]
    [ProducesResponseType(typeof(TeacherExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerateExam(
        int sessionId,
        [FromForm] string? questionCount,
        [FromForm] string? minutesPerQuestion,
        [FromForm(Name = "files")] IFormFileCollection? files,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        if (!int.TryParse(questionCount, out var parsedCount) || parsedCount <= 0)
            parsedCount = 10;

        if (!int.TryParse(minutesPerQuestion, out var parsedMinutes) || parsedMinutes <= 0)
            parsedMinutes = ExamRules.DefaultMinutesPerQuestion;

        var uploads = new List<ExamUploadedFile>();
        foreach (var file in files ?? Enumerable.Empty<IFormFile>())
        {
            if (file.Length <= 0)
                continue;

            await using var stream = file.OpenReadStream();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken);
            uploads.Add(new ExamUploadedFile
            {
                FileName = file.FileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                Content = buffer.ToArray()
            });
        }

        var result = await sender.Send(
            new GenerateSessionExamCommand(
                userId.Value,
                sessionId,
                parsedCount,
                parsedMinutes,
                uploads),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("sessions/{sessionId:int}/exam/publish")]
    [ProducesResponseType(typeof(TeacherExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishExam(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new PublishSessionExamCommand(userId.Value, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("sessions/{sessionId:int}/exam/results")]
    [ProducesResponseType(typeof(TeacherExamResultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExamResults(int sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherSessionExamResultsQuery(userId.Value, sessionId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("sessions/{sessionId:int}/exam/results/{studentId:int}")]
    [HttpGet("sessions/{sessionId:int}/exam/students/{studentId:int}")]
    [ProducesResponseType(typeof(TeacherStudentExamReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentExamReview(
        int sessionId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await sender.Send(
            new GetTeacherStudentExamReviewQuery(userId.Value, sessionId, studentId),
            cancellationToken);

        return result.ToActionResult();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }
}
