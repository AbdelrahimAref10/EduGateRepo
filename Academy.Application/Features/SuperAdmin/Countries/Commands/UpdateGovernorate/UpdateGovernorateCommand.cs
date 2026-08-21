using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateGovernorate;

public sealed record UpdateGovernorateCommand(
    int Id,
    string NameAr,
    string NameEn) : IRequest<Result<GovernorateDto>>;
