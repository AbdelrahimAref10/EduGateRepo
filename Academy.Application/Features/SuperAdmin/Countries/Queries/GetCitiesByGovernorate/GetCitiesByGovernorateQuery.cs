using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetCitiesByGovernorate;

public sealed record GetCitiesByGovernorateQuery(
    int GovernorateId,
    bool ActiveOnly = true) : IRequest<Result<IReadOnlyList<CityDto>>>;
