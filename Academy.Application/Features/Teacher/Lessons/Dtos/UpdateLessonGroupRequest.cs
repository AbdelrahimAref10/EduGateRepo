namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class UpdateLessonGroupRequest
{
    public string Name { get; set; } = null!;

    public List<LessonGroupDateInputDto> Dates { get; set; } = [];

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public int AreaId { get; set; }

    public string Address { get; set; } = null!;

    public string? Notes { get; set; }

    public int? MaxCapacity { get; set; }
}
