namespace Academy.Application.Features.Classroom.Exams;

public static class ExamRules
{
    public const int DefaultMinutesPerQuestion = 10;

    public const int MinMinutesPerQuestion = 1;

    public const int MaxMinutesPerQuestion = 60;

    public const int DefaultSecondsPerQuestion = DefaultMinutesPerQuestion * 60;

    public const int MinQuestionCount = 5;

    public const int MaxQuestionCount = 20;

    public const int MaxFileCount = 10;

    public const long MaxFileBytes = 12 * 1024 * 1024;

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".webp"
    };

    public static bool IsAllowedFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(ext) && AllowedExtensions.Contains(ext);
    }
}
