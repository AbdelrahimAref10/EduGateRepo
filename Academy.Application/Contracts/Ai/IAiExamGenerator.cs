using Academy.Application.Common.Models;

namespace Academy.Application.Contracts.Ai;

public interface IAiExamGenerator
{
    Task<Result<GeneratedExam>> GenerateAsync(
        GenerateExamAiRequest request,
        CancellationToken cancellationToken = default);
}
