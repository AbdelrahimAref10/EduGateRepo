using Academy.Application.Contracts.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Academy.Infrastructure.Storage;

public sealed class ClassroomFileStorage(
    IWebHostEnvironment environment,
    ILogger<ClassroomFileStorage> logger) : IClassroomFileStorage
{
    private const long MaxFileBytes = 50 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx"
    };

    public async Task<StoredClassroomFile> SaveAsync(
        int sessionId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (content.CanSeek && content.Length > MaxFileBytes)
            throw new InvalidOperationException("File exceeds the 50 MB limit.");

        var safeName = Path.GetFileName(originalFileName);
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = "file.bin";

        var ext = Path.GetExtension(safeName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(
                "Only PDF, Word, and Excel files are allowed (.pdf, .doc, .docx, .xls, .xlsx).");

        var folder = Path.Combine(
            environment.ContentRootPath,
            "uploads",
            "classroom",
            sessionId.ToString());

        Directory.CreateDirectory(folder);

        var storedName = $"{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(folder, storedName);

        await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, cancellationToken);
            if (fs.Length > MaxFileBytes)
            {
                fs.Close();
                File.Delete(fullPath);
                throw new InvalidOperationException("File exceeds the 50 MB limit.");
            }
        }

        var relativePath = Path.Combine("uploads", "classroom", sessionId.ToString(), storedName)
            .Replace('\\', '/');

        var info = new FileInfo(fullPath);

        return new StoredClassroomFile
        {
            RelativePath = relativePath,
            OriginalFileName = safeName,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            SizeBytes = info.Length
        };
    }

    public Task<ClassroomFileContent?> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (fullPath is null || !File.Exists(fullPath))
            return Task.FromResult<ClassroomFileContent?>(null);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<ClassroomFileContent?>(new ClassroomFileContent
        {
            Stream = stream,
            ContentType = "application/octet-stream",
            FileName = Path.GetFileName(fullPath)
        });
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (fullPath is null || !File.Exists(fullPath))
            return Task.CompletedTask;

        try
        {
            File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete classroom file {Path}", relativePath);
        }

        return Task.CompletedTask;
    }

    private string? ResolveFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (normalized.Contains("..", StringComparison.Ordinal))
            return null;

        if (!normalized.StartsWith("uploads/classroom/", StringComparison.OrdinalIgnoreCase))
            return null;

        var fullPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, normalized));
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "uploads", "classroom"));

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            return null;

        return fullPath;
    }
}
