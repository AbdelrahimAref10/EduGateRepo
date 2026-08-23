using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroups;

public sealed record GetLessonGroupsQuery(int UserId, int LessonId)
    : IRequest<Result<IReadOnlyList<LessonGroupDto>>>;
