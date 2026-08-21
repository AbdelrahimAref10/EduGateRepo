using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomInfo;

public sealed record UpdateClassroomInfoCommand(
    int UserId,
    int SessionId,
    string? Topic,
    string? Description) : IRequest<Result<TeacherClassroomDto>>;
