using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetMyLessons;

public sealed record GetMyLessonsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<LessonDto>>>;
