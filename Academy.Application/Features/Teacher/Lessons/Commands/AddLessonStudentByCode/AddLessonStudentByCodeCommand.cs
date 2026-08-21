using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.AddLessonStudentByCode;

public sealed record AddLessonStudentByCodeCommand(
    int UserId,
    int LessonId,
    string StudentCode) : IRequest<Result<LessonStudentDto>>;
