namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class CreateLessonGroupRequest
{
    public string Name { get; set; } = null!;

    public List<LessonGroupDateInputDto> Dates { get; set; } = [];

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    /// <summary>
    /// Optional. Defaults to the lesson area when omitted.
    /// </summary>
    public int? AreaId { get; set; }

    public string Address { get; set; } = null!;

    public string? Notes { get; set; }

    public int? MaxCapacity { get; set; }
}
