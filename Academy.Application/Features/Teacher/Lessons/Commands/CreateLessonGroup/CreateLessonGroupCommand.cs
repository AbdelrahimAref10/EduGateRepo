using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.CreateLessonGroup;

public sealed record CreateLessonGroupCommand(
    int UserId,
    int LessonId,
    string Name,
    IReadOnlyList<LessonGroupDateInputDto> Dates,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    int? AreaId,
    string Address,
    string? Notes,
    int? MaxCapacity) : IRequest<Result<LessonGroupDto>>;
