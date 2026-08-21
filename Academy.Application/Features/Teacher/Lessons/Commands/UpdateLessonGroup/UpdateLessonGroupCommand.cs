using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.UpdateLessonGroup;

public sealed record UpdateLessonGroupCommand(
    int UserId,
    int LessonId,
    int GroupId,
    string Name,
    IReadOnlyList<LessonGroupDateInputDto> Dates,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    int AreaId,
    string Address,
    string? Notes,
    int? MaxCapacity) : IRequest<Result<LessonGroupDto>>;
