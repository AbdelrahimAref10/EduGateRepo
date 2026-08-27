using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class UpdateLessonRequest
{
    public int AcademicYearId { get; set; }

    public int EducationStageId { get; set; }

    public int EducationYearId { get; set; }

    public int EducationSubjectId { get; set; }

    public BillingType BillingType { get; set; }

    public decimal? SessionPrice { get; set; }

    public decimal? MonthlyPrice { get; set; }

    public bool ChargeAbsentSessions { get; set; }

    public DateOnly StartDate { get; set; }

    public int AreaId { get; set; }
}
