using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateGovernorate;

public sealed record CreateGovernorateCommand(
    int CountryId,
    string NameAr,
    string NameEn) : IRequest<Result<GovernorateDto>>;
