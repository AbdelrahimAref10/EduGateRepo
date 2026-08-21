using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Countries.Commands.UpdateArea;

public sealed record UpdateAreaCommand(
    int Id,
    string NameAr,
    string NameEn) : IRequest<Result<AreaDto>>;
