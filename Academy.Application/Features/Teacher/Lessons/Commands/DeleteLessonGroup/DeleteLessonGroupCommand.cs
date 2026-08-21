using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.DeleteLessonGroup;

public sealed record DeleteLessonGroupCommand(int UserId, int LessonId, int GroupId) : IRequest<Result>;
