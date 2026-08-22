using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroupSessions;

public sealed record GetLessonGroupSessionsQuery(int UserId, int LessonId, int GroupId)
    : IRequest<Result<IReadOnlyList<LessonGroupSessionDto>>>;
