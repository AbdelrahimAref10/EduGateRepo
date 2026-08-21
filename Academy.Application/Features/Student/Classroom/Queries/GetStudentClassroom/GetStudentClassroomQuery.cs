using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentClassroom;

public sealed record GetStudentClassroomQuery(
    int UserId,
    int SessionId) : IRequest<Result<StudentClassroomDto>>;
