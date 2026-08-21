using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.CreateLesson;

public sealed record CreateLessonCommand(
    int UserId,
    int EducationTypeId,
    int EducationStageId,
    int EducationYearId,
    int EducationSubjectId,
    BillingType BillingType,
    decimal? SessionPrice,
    decimal? MonthlyPrice,
    DateOnly StartDate,
    int AreaId) : IRequest<Result<LessonDto>>;
