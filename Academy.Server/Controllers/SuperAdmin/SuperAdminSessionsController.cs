using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminClassroom;
using Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminClassroomMaterialFile;
using Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminSessionExamResults;
using Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminSessionReviews;
using Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminStudentExamReview;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Authorize(Roles = AppRoles.SuperAdmin)]
[Route("api/super-admin/sessions")]
[Produces("application/json")]
public sealed class SuperAdminSessionsController(ISender sender) : ControllerBase
{
    [HttpGet("{sessionId:int}/classroom")]
    [ProducesResponseType(typeof(AdminClassroomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetClassroom(int sessionId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminClassroomQuery(sessionId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{sessionId:int}/materials/{materialId:int}/file")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadMaterialFile(
        int sessionId,
        int materialId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminClassroomMaterialFileQuery(sessionId, materialId),
            cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var file = result.Value!;
        return File(file.Stream, file.ContentType, file.FileName);
    }

    [HttpGet("{sessionId:int}/reviews")]
    [ProducesResponseType(typeof(AdminReviewsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionReviews(int sessionId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminSessionReviewsQuery(sessionId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{sessionId:int}/exam/results")]
    [ProducesResponseType(typeof(TeacherExamResultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetExamResults(int sessionId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminSessionExamResultsQuery(sessionId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{sessionId:int}/exam/results/{studentId:int}")]
    [ProducesResponseType(typeof(TeacherStudentExamReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetStudentExamReview(
        int sessionId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminStudentExamReviewQuery(sessionId, studentId),
            cancellationToken);

        return result.ToActionResult();
    }
}
