using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Students.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Students.Queries.GetTeacherStudentLessons;

public sealed record GetTeacherStudentLessonsQuery(int UserId, int StudentId)
    : IRequest<Result<IReadOnlyList<TeacherStudentLessonDto>>>;
