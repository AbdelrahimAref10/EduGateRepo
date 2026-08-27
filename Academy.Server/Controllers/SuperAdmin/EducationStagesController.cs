using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationStage;
using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationSubject;
using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationStage;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationSubject;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationStage;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationSubject;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationStages;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationSubjectsByYear;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYears;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Route("api/super-admin/education-stages")]
[Produces("application/json")]
public sealed class EducationStagesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EducationStageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStages(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetEducationStagesQuery(activeOnly), cancellationToken);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("{stageId:int}/years")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationYearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetYears(
        int stageId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetEducationYearsQuery(stageId, activeOnly), cancellationToken);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("{stageId:int}/years/{yearId:int}/subjects")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationSubjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjects(
        int stageId,
        int yearId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetEducationSubjectsByYearQuery(stageId, yearId, activeOnly),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(EducationStageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateStage(
        [FromBody] CreateEducationStageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationStageCommand(request.NameAr, request.NameEn, request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{stageId:int}/years")]
    [ProducesResponseType(typeof(EducationYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateYear(
        int stageId,
        [FromBody] CreateEducationYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationYearCommand(
                stageId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{stageId:int}/years/{yearId:int}/subjects")]
    [ProducesResponseType(typeof(EducationSubjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSubject(
        int stageId,
        int yearId,
        [FromBody] CreateEducationSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationSubjectCommand(
                stageId,
                yearId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{stageId:int}")]
    [ProducesResponseType(typeof(EducationStageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStage(
        int stageId,
        [FromBody] UpdateEducationStageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationStageCommand(stageId, request.NameAr, request.NameEn, request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{stageId:int}/years/{yearId:int}")]
    [ProducesResponseType(typeof(EducationYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateYear(
        int stageId,
        int yearId,
        [FromBody] UpdateEducationYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationYearCommand(
                stageId,
                yearId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{stageId:int}/years/{yearId:int}/subjects/{subjectId:int}")]
    [ProducesResponseType(typeof(EducationSubjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSubject(
        int stageId,
        int yearId,
        int subjectId,
        [FromBody] UpdateEducationSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationSubjectCommand(
                stageId,
                yearId,
                subjectId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{stageId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteStage(int stageId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteEducationStageCommand(stageId), cancellationToken);
        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{stageId:int}/years/{yearId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteYear(int stageId, int yearId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteEducationYearCommand(stageId, yearId), cancellationToken);
        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{stageId:int}/years/{yearId:int}/subjects/{subjectId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteSubject(
        int stageId,
        int yearId,
        int subjectId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteEducationSubjectCommand(stageId, yearId, subjectId),
            cancellationToken);

        return result.ToActionResult();
    }
}
