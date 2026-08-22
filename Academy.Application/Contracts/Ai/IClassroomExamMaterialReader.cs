namespace Academy.Application.Contracts.Ai;

public interface IClassroomExamMaterialReader
{
    Task<IReadOnlyList<ExamSourceMaterial>> ReadUploadedAsync(
        IReadOnlyList<ExamUploadedFile> files,
        CancellationToken cancellationToken = default);
}
