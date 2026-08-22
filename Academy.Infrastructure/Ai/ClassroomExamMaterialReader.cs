using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Academy.Application.Contracts.Ai;
using Academy.Application.Features.Classroom.Exams;

namespace Academy.Infrastructure.Ai;

public sealed class ClassroomExamMaterialReader : IClassroomExamMaterialReader
{
    public Task<IReadOnlyList<ExamSourceMaterial>> ReadUploadedAsync(
        IReadOnlyList<ExamUploadedFile> files,
        CancellationToken cancellationToken = default)
    {
        var sources = new List<ExamSourceMaterial>();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Content.Length == 0)
                continue;

            var ext = Path.GetExtension(file.FileName);
            var title = Path.GetFileNameWithoutExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(title))
                title = file.FileName;

            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                sources.Add(new ExamSourceMaterial
                {
                    Title = title,
                    Text = $"PDF file uploaded by the teacher: {file.FileName}",
                    FileBytes = file.Content.Length <= ExamRules.MaxFileBytes ? file.Content : null,
                    MimeType = "application/pdf",
                    FileName = file.FileName
                });
                continue;
            }

            if (IsImage(ext))
            {
                sources.Add(new ExamSourceMaterial
                {
                    Title = title,
                    Text = $"Image uploaded by the teacher: {file.FileName}. Read the image and write questions from what is written or shown in it.",
                    FileBytes = file.Content.Length <= ExamRules.MaxFileBytes ? file.Content : null,
                    MimeType = ResolveImageMime(ext, file.ContentType),
                    FileName = file.FileName
                });
                continue;
            }

            if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".doc", StringComparison.OrdinalIgnoreCase))
            {
                var extracted = TryExtractDocxText(file.Content);
                if (string.IsNullOrWhiteSpace(extracted))
                    continue;

                sources.Add(new ExamSourceMaterial
                {
                    Title = title,
                    Text = extracted,
                    FileName = file.FileName
                });
            }
        }

        return Task.FromResult<IReadOnlyList<ExamSourceMaterial>>(sources);
    }

    private static bool IsImage(string ext) =>
        ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".webp", StringComparison.OrdinalIgnoreCase);

    private static string ResolveImageMime(string ext, string contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return contentType;

        return ext.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    private static string? TryExtractDocxText(byte[] content)
    {
        try
        {
            using var copy = new MemoryStream(content, writable: false);
            using var zip = new ZipArchive(copy, ZipArchiveMode.Read, leaveOpen: false);
            var entry = zip.GetEntry("word/document.xml");
            if (entry is null)
                return null;

            using var xmlStream = entry.Open();
            using var reader = new StreamReader(xmlStream, Encoding.UTF8);
            var xml = reader.ReadToEnd();

            var withBreaks = Regex.Replace(xml, "</w:p>", "\n");
            var withoutTags = Regex.Replace(withBreaks, "<[^>]+>", " ");
            var decoded = WebUtility.HtmlDecode(withoutTags);
            var collapsed = Regex.Replace(decoded, @"[ \t]+", " ");
            collapsed = Regex.Replace(collapsed, @"\n{3,}", "\n\n");
            return collapsed.Trim();
        }
        catch
        {
            return null;
        }
    }
}
