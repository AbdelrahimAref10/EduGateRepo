using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Queries.GetAreasByCity;

public sealed record GetAreasByCityQuery(
    int CityId,
    bool ActiveOnly = true) : IRequest<Result<IReadOnlyList<AreaDto>>>;
