namespace Academy.Application.Contracts.Ai;

public sealed class ExamGenerationProgressDto
{
    public required string Step { get; init; }

    public required int Current { get; init; }

    public required int Total { get; init; }

    public required int Percent { get; init; }
}

public interface IExamGenerationProgress
{
    Task ReportAsync(int userId, ExamGenerationProgressDto progress, CancellationToken cancellationToken = default);
}

public sealed class NullExamGenerationProgress : IExamGenerationProgress
{
    public Task ReportAsync(
        int userId,
        ExamGenerationProgressDto progress,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

public static class ExamGenerationSteps
{
    public const int Total = 5;

    public static ExamGenerationProgressDto Read() => new()
    {
        Step = "read",
        Current = 1,
        Total = Total,
        Percent = 12
    };

    public static ExamGenerationProgressDto Prepare() => new()
    {
        Step = "prepare",
        Current = 2,
        Total = Total,
        Percent = 28
    };

    public static ExamGenerationProgressDto Generate() => new()
    {
        Step = "generate",
        Current = 3,
        Total = Total,
        Percent = 48
    };

    public static ExamGenerationProgressDto Save() => new()
    {
        Step = "save",
        Current = 4,
        Total = Total,
        Percent = 86
    };

    public static ExamGenerationProgressDto Done() => new()
    {
        Step = "done",
        Current = 5,
        Total = Total,
        Percent = 100
    };
}
