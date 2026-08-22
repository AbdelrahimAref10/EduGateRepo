using Academy.Domain.Enums;

namespace Academy.Application.Contracts.Ai;

public sealed class ExamSourceMaterial
{
    public required string Title { get; init; }

    public string? Text { get; init; }

    public byte[]? FileBytes { get; init; }

    public string? MimeType { get; init; }

    public string? FileName { get; init; }
}

public sealed class GenerateExamAiRequest
{
    public required IReadOnlyList<ExamSourceMaterial> Materials { get; init; }

    public required int QuestionCount { get; init; }

    public string? Subject { get; init; }

    public string? Topic { get; init; }

    public AppLanguage Language { get; init; } = AppLanguage.Arabic;
}

public sealed class GeneratedExamQuestion
{
    public required string Text { get; init; }

    public required IReadOnlyList<GeneratedExamOption> Options { get; init; }
}

public sealed class GeneratedExamOption
{
    public required string Text { get; init; }

    public required bool IsCorrect { get; init; }
}

public sealed class GeneratedExam
{
    public required string Title { get; init; }

    public required IReadOnlyList<GeneratedExamQuestion> Questions { get; init; }
}
