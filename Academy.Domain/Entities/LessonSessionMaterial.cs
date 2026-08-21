using Academy.Domain.Common;
using Academy.Domain.Enums;

namespace Academy.Domain.Entities;

/// <summary>
/// Classroom material: note, link, uploaded file, recording, or assignment.
/// </summary>
public class LessonSessionMaterial : BaseEntity
{
    public int LessonGroupSessionId { get; set; }

    public LessonGroupSession LessonGroupSession { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public ClassroomMaterialType MaterialType { get; set; }

    /// <summary>External URL (links / online recordings).</summary>
    public string? ExternalUrl { get; set; }

    /// <summary>Relative stored path under uploads for uploaded files.</summary>
    public string? StoredFilePath { get; set; }

    public string? OriginalFileName { get; set; }

    public string? ContentType { get; set; }

    public long? FileSizeBytes { get; set; }

    /// <summary>Rich text / note body for Note type.</summary>
    public string? Body { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
