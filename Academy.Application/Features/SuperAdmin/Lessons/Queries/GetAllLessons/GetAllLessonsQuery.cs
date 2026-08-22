using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Lessons.Queries.GetAllLessons;

public sealed record GetAllLessonsQuery : IRequest<Result<IReadOnlyList<AdminLessonListItemDto>>>;
