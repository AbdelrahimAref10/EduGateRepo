using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonStudents;

public sealed record GetLessonStudentsQuery(int UserId, int LessonId)
    : IRequest<Result<IReadOnlyList<LessonStudentDto>>>;
