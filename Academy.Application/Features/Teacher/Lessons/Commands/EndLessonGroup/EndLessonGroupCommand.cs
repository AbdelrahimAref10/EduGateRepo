using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.EndLessonGroup;

public sealed record EndLessonGroupCommand(int UserId, int LessonId, int GroupId)
    : IRequest<Result<LessonGroupDto>>;
