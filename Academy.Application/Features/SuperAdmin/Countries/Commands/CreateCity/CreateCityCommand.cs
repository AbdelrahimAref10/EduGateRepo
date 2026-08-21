using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateCity;

public sealed record CreateCityCommand(
    int GovernorateId,
    string NameAr,
    string NameEn) : IRequest<Result<CityDto>>;
