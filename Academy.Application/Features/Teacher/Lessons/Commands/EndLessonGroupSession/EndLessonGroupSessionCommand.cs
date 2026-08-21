using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.EndLessonGroupSession;

public sealed record EndLessonGroupSessionCommand(
    int UserId,
    int LessonId,
    int GroupId,
    int SessionId) : IRequest<Result<LessonGroupSessionDto>>;
