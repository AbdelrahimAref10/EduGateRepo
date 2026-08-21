namespace Academy.Application.Contracts.Storage;

public sealed class StoredClassroomFile
{
    public required string RelativePath { get; init; }

    public required string OriginalFileName { get; init; }

    public required string ContentType { get; init; }

    public required long SizeBytes { get; init; }
}

public sealed class ClassroomFileContent
{
    public required Stream Stream { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }
}

public interface IClassroomFileStorage
{
    Task<StoredClassroomFile> SaveAsync(
        int sessionId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<ClassroomFileContent?> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
