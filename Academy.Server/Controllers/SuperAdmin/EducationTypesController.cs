using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationStage;
using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationSubject;
using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationType;
using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationStage;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationSubject;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationType;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationStage;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationSubject;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationType;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationStagesByType;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationSubjectsByYear;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationTypes;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYearsByStage;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Route("api/super-admin/education-types")]
[Produces("application/json")]
public sealed class EducationTypesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EducationTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypes(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetEducationTypesQuery(activeOnly), cancellationToken);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("{educationTypeId:int}/stages")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationStageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStages(
        int educationTypeId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetEducationStagesByTypeQuery(educationTypeId, activeOnly),
            cancellationToken);

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("{educationTypeId:int}/stages/{stageId:int}/years")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationYearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetYears(
        int educationTypeId,
        int stageId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetEducationYearsByStageQuery(educationTypeId, stageId, activeOnly),
            cancellationToken);

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("{educationTypeId:int}/stages/{stageId:int}/years/{yearId:int}/subjects")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationSubjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjects(
        int educationTypeId,
        int stageId,
        int yearId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetEducationSubjectsByYearQuery(educationTypeId, stageId, yearId, activeOnly),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(EducationTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateType(
        [FromBody] CreateEducationTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationTypeCommand(request.NameAr, request.NameEn, request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{educationTypeId:int}/stages")]
    [ProducesResponseType(typeof(EducationStageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateStage(
        int educationTypeId,
        [FromBody] CreateEducationStageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationStageCommand(
                educationTypeId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{educationTypeId:int}/stages/{stageId:int}/years")]
    [ProducesResponseType(typeof(EducationYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateYear(
        int educationTypeId,
        int stageId,
        [FromBody] CreateEducationYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationYearCommand(
                educationTypeId,
                stageId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{educationTypeId:int}/stages/{stageId:int}/years/{yearId:int}/subjects")]
    [ProducesResponseType(typeof(EducationSubjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSubject(
        int educationTypeId,
        int stageId,
        int yearId,
        [FromBody] CreateEducationSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationSubjectCommand(
                educationTypeId,
                stageId,
                yearId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{educationTypeId:int}")]
    [ProducesResponseType(typeof(EducationTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateType(
        int educationTypeId,
        [FromBody] UpdateEducationTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationTypeCommand(
                educationTypeId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{educationTypeId:int}/stages/{stageId:int}")]
    [ProducesResponseType(typeof(EducationStageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStage(
        int educationTypeId,
        int stageId,
        [FromBody] UpdateEducationStageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationStageCommand(
                educationTypeId,
                stageId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{educationTypeId:int}/stages/{stageId:int}/years/{yearId:int}")]
    [ProducesResponseType(typeof(EducationYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateYear(
        int educationTypeId,
        int stageId,
        int yearId,
        [FromBody] UpdateEducationYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationYearCommand(
                educationTypeId,
                stageId,
                yearId,
                request.NameAr,
                request.NameEn,
                request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{educationTypeId:int}/stages/{stageId:int}/years/{yearId:int}/subjects/{subjectId:int}")]
    [ProducesResponseType(typeof(EducationSubjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSubject(
        int educationTypeId,
        int stageId,
        int yearId,
        int subjectId,
        [FromBody] UpdateEducationSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationSubjectCommand(
                educationTypeId,
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
    [HttpDelete("{educationTypeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteType(int educationTypeId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteEducationTypeCommand(educationTypeId), cancellationToken);
        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{educationTypeId:int}/stages/{stageId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteStage(
        int educationTypeId,
        int stageId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteEducationStageCommand(educationTypeId, stageId),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{educationTypeId:int}/stages/{stageId:int}/years/{yearId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteYear(
        int educationTypeId,
        int stageId,
        int yearId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteEducationYearCommand(educationTypeId, stageId, yearId),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{educationTypeId:int}/stages/{stageId:int}/years/{yearId:int}/subjects/{subjectId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteSubject(
        int educationTypeId,
        int stageId,
        int yearId,
        int subjectId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteEducationSubjectCommand(educationTypeId, stageId, yearId, subjectId),
            cancellationToken);

        return result.ToActionResult();
    }
}
