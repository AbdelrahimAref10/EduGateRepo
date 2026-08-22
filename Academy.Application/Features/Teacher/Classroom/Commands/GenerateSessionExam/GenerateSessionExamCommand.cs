using Academy.Application.Common.Models;
using Academy.Application.Contracts.Ai;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.GenerateSessionExam;

public sealed record GenerateSessionExamCommand(
    int UserId,
    int SessionId,
    int QuestionCount,
    int MinutesPerQuestion,
    IReadOnlyList<ExamUploadedFile> Files) : IRequest<Result<TeacherExamDto>>;
