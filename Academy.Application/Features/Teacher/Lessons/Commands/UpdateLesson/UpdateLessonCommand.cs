using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.UpdateLesson;

public sealed record UpdateLessonCommand(
    int UserId,
    int LessonId,
    string Subject,
    int EducationTypeId,
    int EducationYearId,
    BillingType BillingType,
    decimal? SessionPrice,
    decimal? MonthlyPrice,
    DateOnly StartDate,
    int AreaId) : IRequest<Result<LessonDto>>;
