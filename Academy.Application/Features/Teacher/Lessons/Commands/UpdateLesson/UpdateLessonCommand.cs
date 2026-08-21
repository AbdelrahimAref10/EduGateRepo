using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.UpdateLesson;

public sealed record UpdateLessonCommand(
    int UserId,
    int LessonId,
    int EducationTypeId,
    int EducationStageId,
    int EducationYearId,
    int EducationSubjectId,
    BillingType BillingType,
    decimal? SessionPrice,
    decimal? MonthlyPrice,
    DateOnly StartDate,
    int AreaId) : IRequest<Result<LessonDto>>;
