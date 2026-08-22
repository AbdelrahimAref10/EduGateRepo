using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Classroom.Commands.StartStudentSessionExam;

public sealed record StartStudentSessionExamCommand(int UserId, int SessionId)
    : IRequest<Result<StudentExamDto>>;
