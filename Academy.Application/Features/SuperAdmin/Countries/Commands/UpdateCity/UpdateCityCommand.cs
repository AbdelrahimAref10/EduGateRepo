using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateCity;

public sealed record UpdateCityCommand(
    int Id,
    string NameAr,
    string NameEn) : IRequest<Result<CityDto>>;
