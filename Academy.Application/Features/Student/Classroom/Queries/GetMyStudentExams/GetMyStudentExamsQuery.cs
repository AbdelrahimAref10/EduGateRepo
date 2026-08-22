using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Classroom.Queries.GetMyStudentExams;

public sealed record GetMyStudentExamsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<StudentExamListItemDto>>>;
