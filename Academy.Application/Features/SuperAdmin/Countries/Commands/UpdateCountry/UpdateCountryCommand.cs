using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateCountry;

public sealed record UpdateCountryCommand(
    int Id,
    string NameAr,
    string NameEn,
    string Code) : IRequest<Result<CountryDto>>;
