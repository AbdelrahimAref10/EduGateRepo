using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroup;

public sealed record GetLessonGroupQuery(int UserId, int LessonId, int GroupId)
    : IRequest<Result<LessonGroupDto>>;
