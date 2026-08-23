using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateStudentSessionDetail;

public sealed record UpdateStudentSessionDetailCommand(
    int UserId,
    int SessionId,
    int StudentId,
    bool IsPresent,
    string? TeacherNotes) : IRequest<Result<ClassroomStudentDetailDto>>;
