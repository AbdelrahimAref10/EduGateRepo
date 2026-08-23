using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminGroupSessions;

public sealed record GetAdminGroupSessionsQuery(int GroupId)
    : IRequest<Result<AdminGroupSessionsDto>>;
