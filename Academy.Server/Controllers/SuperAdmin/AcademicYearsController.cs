using Academy.Application.Features.SuperAdmin.Education.Commands.CreateAcademicYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.DeleteAcademicYear;
using Academy.Application.Features.SuperAdmin.Education.Commands.UpdateAcademicYear;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Application.Features.SuperAdmin.Education.Queries.GetAcademicYears;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Route("api/super-admin/academic-years")]
[Produces("application/json")]
public sealed class AcademicYearsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AcademicYearDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetAcademicYearsQuery(activeOnly), cancellationToken);
        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(AcademicYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateAcademicYearCommand(request.Name, request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AcademicYearDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateAcademicYearCommand(id, request.Name, request.SortOrder),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAcademicYearCommand(id), cancellationToken);
        return result.ToActionResult();
    }
}
