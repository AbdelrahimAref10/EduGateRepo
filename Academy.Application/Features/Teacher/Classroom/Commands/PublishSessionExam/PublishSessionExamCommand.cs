using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.PublishSessionExam;

public sealed record PublishSessionExamCommand(int UserId, int SessionId)
    : IRequest<Result<TeacherExamDto>>;
