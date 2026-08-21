namespace Academy.Application.Features.Teacher.Lessons.Dtos;

public sealed class AddGroupMemberRequest
{
    /// <summary>
    /// Preferred when selecting from the booked-students list.
    /// </summary>
    public int? StudentId { get; set; }

    /// <summary>
    /// Used for manual code entry (lesson manage page).
    /// </summary>
    public string? StudentCode { get; set; }
}
