using Academy.Domain.Common;

namespace Academy.Domain.Entities;

/// <summary>
/// Student assignment to a lesson group (typically added by student code).
/// </summary>
public class LessonGroupMember : BaseEntity
{
    public int LessonGroupId { get; set; }

    public LessonGroup LessonGroup { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Moves this student to another group of the same lesson.
    /// Current <see cref="LessonGroup"/> must be loaded.
    /// <paramref name="targetGroup"/> must have <see cref="LessonGroup.Members"/> loaded for capacity checks.
    /// </summary>
    /// <returns>An error message, or <c>null</c> when the move succeeded.</returns>
    public string? TryMoveTo(LessonGroup targetGroup)
    {
        ArgumentNullException.ThrowIfNull(targetGroup);

        if (LessonGroup is null)
            throw new InvalidOperationException("Current group must be loaded before moving the student.");

        if (targetGroup.Id == LessonGroupId)
            return "الطالب موجود بالفعل في هذه المجموعة.";

        if (targetGroup.LessonId != LessonGroup.LessonId)
            return "لا يمكن نقل الطالب إلى مجموعة في درس آخر.";

        if (targetGroup.HasEnded)
            return "لا يمكن نقل الطالب إلى مجموعة منتهية.";

        if (targetGroup.IsFull)
            return "المجموعة ممتلئة.";

        if (targetGroup.Members.Any(m => m.StudentId == StudentId && m.Id != Id))
            return "الطالب موجود بالفعل في هذه المجموعة.";

        LessonGroupId = targetGroup.Id;
        LessonGroup = targetGroup;
        return null;
    }
}
