using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class CreateLessonRequest
{
    public string Subject { get; set; } = null!;

    public int EducationTypeId { get; set; }

    public int EducationYearId { get; set; }

    public BillingType BillingType { get; set; }

    public decimal? SessionPrice { get; set; }

    public decimal? MonthlyPrice { get; set; }

    public DateOnly StartDate { get; set; }

    public int AreaId { get; set; }
}
