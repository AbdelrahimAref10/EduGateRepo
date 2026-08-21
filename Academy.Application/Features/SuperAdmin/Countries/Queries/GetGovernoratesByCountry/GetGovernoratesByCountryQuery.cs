using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetGovernoratesByCountry;

public sealed record GetGovernoratesByCountryQuery(
    int CountryId,
    bool ActiveOnly = true) : IRequest<Result<IReadOnlyList<GovernorateDto>>>;
