using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.StartLessonGroupSession;

public sealed record StartLessonGroupSessionCommand(
    int UserId,
    int LessonId,
    int GroupId,
    int SessionId) : IRequest<Result<LessonGroupSessionDto>>;
