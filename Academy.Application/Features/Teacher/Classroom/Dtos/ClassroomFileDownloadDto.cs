namespace Academy.Application.Features.Teacher.Classroom.Dtos;

public sealed class ClassroomFileDownloadDto
{
    public required Stream Stream { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }
}
