using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Lessons.Queries.GetAvailableLessons;

public sealed record GetAvailableLessonsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<AvailableLessonDto>>>;
