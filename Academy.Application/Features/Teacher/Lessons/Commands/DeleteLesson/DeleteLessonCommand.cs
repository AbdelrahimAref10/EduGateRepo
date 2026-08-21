using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.DeleteLesson;

public sealed record DeleteLessonCommand(int UserId, int LessonId) : IRequest<Result>;
