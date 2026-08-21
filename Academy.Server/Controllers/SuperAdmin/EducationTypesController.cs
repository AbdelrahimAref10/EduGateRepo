using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationType;
using Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationType;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationType;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationYear;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationTypes;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetEducationYearsByType;
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
    [HttpGet("{educationTypeId:int}/years")]
    [ProducesResponseType(typeof(IReadOnlyList<EducationYearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetYears(
        int educationTypeId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetEducationYearsByTypeQuery(educationTypeId, activeOnly),
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
    [HttpPost("{educationTypeId:int}/years")]
    [ProducesResponseType(typeof(EducationYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateYear(
        int educationTypeId,
        [FromBody] CreateEducationYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateEducationYearCommand(
                educationTypeId,
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
    [HttpPut("{educationTypeId:int}/years/{yearId:int}")]
    [ProducesResponseType(typeof(EducationYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateYear(
        int educationTypeId,
        int yearId,
        [FromBody] UpdateEducationYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateEducationYearCommand(
                educationTypeId,
                yearId,
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
    [HttpDelete("{educationTypeId:int}/years/{yearId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteYear(
        int educationTypeId,
        int yearId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteEducationYearCommand(educationTypeId, yearId),
            cancellationToken);

        return result.ToActionResult();
    }
}
