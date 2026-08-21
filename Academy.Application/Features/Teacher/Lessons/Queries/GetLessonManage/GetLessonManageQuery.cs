using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonManage;

public sealed record GetLessonManageQuery(int UserId, int LessonId) : IRequest<Result<LessonManageDto>>;
