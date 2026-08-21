using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Countries.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetMyCityAreas;

public sealed record GetMyCityAreasQuery(int UserId) : IRequest<Result<IReadOnlyList<AreaDto>>>;
