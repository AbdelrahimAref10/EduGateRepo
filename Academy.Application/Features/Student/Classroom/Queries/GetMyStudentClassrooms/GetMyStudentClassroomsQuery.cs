using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Classroom.Queries.GetMyStudentClassrooms;

public sealed record GetMyStudentClassroomsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<StudentClassroomSessionListItemDto>>>;
