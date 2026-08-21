using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.CreateArea;

public sealed record CreateAreaCommand(
    int CityId,
    string NameAr,
    string NameEn) : IRequest<Result<AreaDto>>;
