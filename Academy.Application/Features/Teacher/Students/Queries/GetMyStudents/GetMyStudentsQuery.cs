using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Students.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Students.Queries.GetMyStudents;

public sealed record GetMyStudentsQuery(int UserId, string? Search)
    : IRequest<Result<IReadOnlyList<TeacherStudentListItemDto>>>;
