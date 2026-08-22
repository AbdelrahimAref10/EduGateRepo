using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Classroom.Commands.SubmitStudentSessionExam;

public sealed record SubmitStudentSessionExamCommand(
    int UserId,
    int SessionId,
    int? OptionId) : IRequest<Result<StudentExamDto>>;
