namespace Academy.Application.Features.Teacher.Classroom.Dtos;

public sealed class ClassroomMaterialDto
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required int MaterialType { get; init; }

    public required string MaterialTypeName { get; init; }

    public string? ExternalUrl { get; init; }

    public string? OriginalFileName { get; init; }

    public string? ContentType { get; init; }

    public long? FileSizeBytes { get; init; }

    public string? Body { get; init; }

    public required int SortOrder { get; init; }

    public required bool HasFile { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public required string CreatedByName { get; init; }
}
