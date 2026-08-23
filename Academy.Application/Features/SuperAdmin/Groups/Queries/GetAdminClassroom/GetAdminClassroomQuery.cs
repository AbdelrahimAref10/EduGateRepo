using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminClassroom;

public sealed record GetAdminClassroomQuery(int SessionId)
    : IRequest<Result<AdminClassroomDto>>;
