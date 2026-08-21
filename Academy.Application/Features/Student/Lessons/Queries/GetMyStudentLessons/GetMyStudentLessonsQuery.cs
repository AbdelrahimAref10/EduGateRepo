using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Lessons.Queries.GetMyStudentLessons;

public sealed record GetMyStudentLessonsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<StudentLessonListItemDto>>>;
