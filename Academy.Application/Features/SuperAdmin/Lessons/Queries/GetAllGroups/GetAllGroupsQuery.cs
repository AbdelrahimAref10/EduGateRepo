using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Lessons.Queries.GetAllGroups;

public sealed record GetAllGroupsQuery : IRequest<Result<IReadOnlyList<AdminGroupListItemDto>>>;
