using Academy.Application.Features.SuperAdmin.Countries.Commands.CreateArea;
using Academy.Application.Features.SuperAdmin.Countries.Commands.CreateCity;
using Academy.Application.Features.SuperAdmin.Countries.Commands.CreateCountry;
using Academy.Application.Features.SuperAdmin.Countries.Commands.CreateGovernorate;
using Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteArea;
using Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteCity;
using Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteCountry;
using Academy.Application.Features.SuperAdmin.Countries.Commands.DeleteGovernorate;
using Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateArea;
using Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateCity;
using Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateCountry;
using Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateGovernorate;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using Academy.Application.Features.SuperAdmin.Countries.Queries.GetAreasByCity;
using Academy.Application.Features.SuperAdmin.Countries.Queries.GetCitiesByGovernorate;
using Academy.Application.Features.SuperAdmin.Countries.Queries.GetCountries;
using Academy.Application.Features.SuperAdmin.Countries.Queries.GetGovernoratesByCountry;
using Academy.Domain.Common;
using Academy.Server.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Server.Controllers.SuperAdmin;

[ApiController]
[Route("api/super-admin/countries")]
[Produces("application/json")]
public sealed class CountriesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CountryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCountriesQuery(activeOnly), cancellationToken);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("{countryId:int}/governorates")]
    [ProducesResponseType(typeof(IReadOnlyList<GovernorateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGovernorates(
        int countryId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetGovernoratesByCountryQuery(countryId, activeOnly),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCountry(
        [FromBody] CreateCountryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateCountryCommand(request.NameAr, request.NameEn, request.Code),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{countryId:int}")]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCountry(
        int countryId,
        [FromBody] CreateCountryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCountryCommand(countryId, request.NameAr, request.NameEn, request.Code),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{countryId:int}/governorates")]
    [ProducesResponseType(typeof(GovernorateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateGovernorate(
        int countryId,
        [FromBody] CreateLocationNameRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateGovernorateCommand(countryId, request.NameAr, request.NameEn),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{countryId:int}/governorates/{governorateId:int}")]
    [ProducesResponseType(typeof(GovernorateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateGovernorate(
        int countryId,
        int governorateId,
        [FromBody] CreateLocationNameRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateGovernorateCommand(governorateId, request.NameAr, request.NameEn),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{countryId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCountry(int countryId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCountryCommand(countryId), cancellationToken);
        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{countryId:int}/governorates/{governorateId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteGovernorate(
        int countryId,
        int governorateId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteGovernorateCommand(governorateId), cancellationToken);
        return result.ToActionResult();
    }
}

[ApiController]
[Route("api/super-admin/governorates")]
[Produces("application/json")]
public sealed class GovernoratesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{governorateId:int}/cities")]
    [ProducesResponseType(typeof(IReadOnlyList<CityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCities(
        int governorateId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetCitiesByGovernorateQuery(governorateId, activeOnly),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{governorateId:int}/cities")]
    [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCity(
        int governorateId,
        [FromBody] CreateLocationNameRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateCityCommand(governorateId, request.NameAr, request.NameEn),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{governorateId:int}/cities/{cityId:int}")]
    [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCity(
        int governorateId,
        int cityId,
        [FromBody] CreateLocationNameRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCityCommand(cityId, request.NameAr, request.NameEn),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{governorateId:int}/cities/{cityId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCity(
        int governorateId,
        int cityId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCityCommand(cityId), cancellationToken);
        return result.ToActionResult();
    }
}

[ApiController]
[Route("api/super-admin/cities")]
[Produces("application/json")]
public sealed class CitiesController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{cityId:int}/areas")]
    [ProducesResponseType(typeof(IReadOnlyList<AreaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAreas(
        int cityId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetAreasByCityQuery(cityId, activeOnly),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPost("{cityId:int}/areas")]
    [ProducesResponseType(typeof(AreaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateArea(
        int cityId,
        [FromBody] CreateLocationNameRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateAreaCommand(cityId, request.NameAr, request.NameEn),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpPut("{cityId:int}/areas/{areaId:int}")]
    [ProducesResponseType(typeof(AreaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateArea(
        int cityId,
        int areaId,
        [FromBody] CreateLocationNameRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateAreaCommand(areaId, request.NameAr, request.NameEn),
            cancellationToken);

        return result.ToActionResult();
    }

    [Authorize(Roles = AppRoles.SuperAdmin)]
    [HttpDelete("{cityId:int}/areas/{areaId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteArea(
        int cityId,
        int areaId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAreaCommand(areaId), cancellationToken);
        return result.ToActionResult();
    }
}
