using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Lessons.Commands.CreateLesson;

public sealed record CreateLessonCommand(
    int UserId,
    int AcademicYearId,
    int EducationStageId,
    int EducationYearId,
    int EducationSubjectId,
    BillingType BillingType,
    decimal? SessionPrice,
    decimal? MonthlyPrice,
    bool ChargeAbsentSessions,
    DateOnly StartDate,
    int AreaId) : IRequest<Result<LessonDto>>;
