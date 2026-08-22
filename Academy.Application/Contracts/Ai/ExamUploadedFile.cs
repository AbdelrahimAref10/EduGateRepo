namespace Academy.Application.Contracts.Ai;

public sealed class ExamUploadedFile
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required byte[] Content { get; init; }
}
