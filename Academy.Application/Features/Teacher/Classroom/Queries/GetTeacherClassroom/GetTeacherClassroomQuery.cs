using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherClassroom;

public sealed record GetTeacherClassroomQuery(
    int UserId,
    int SessionId) : IRequest<Result<TeacherClassroomDto>>;
