using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateCountry;

public sealed record CreateCountryCommand(
    string NameAr,
    string NameEn,
    string Code) : IRequest<Result<CountryDto>>;
