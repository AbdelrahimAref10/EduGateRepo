using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.StartLesson;

public sealed record StartLessonCommand(int UserId, int LessonId) : IRequest<Result<LessonDto>>;
