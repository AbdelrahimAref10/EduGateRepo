using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetCountries;

public sealed record GetCountriesQuery(bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<CountryDto>>>;
