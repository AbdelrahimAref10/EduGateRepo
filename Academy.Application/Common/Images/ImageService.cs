using Academy.Application.Common.Models;
using Academy.Application.Contracts.Images;

namespace Academy.Application.Common.Images;

public sealed class ImageService : IImageService
{
    public const int MaxBytes = 5 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, byte[][]> Signatures = new Dictionary<string, byte[][]>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = [[0xFF, 0xD8, 0xFF]],
        ["image/png"] = [[0x89, 0x50, 0x4E, 0x47]],
        ["image/webp"] = [[0x52, 0x49, 0x46, 0x46]]
    };

    public string? Display(string? photo) => DisplayValue(photo);

    public static string? DisplayValue(string? photo) =>
        string.IsNullOrWhiteSpace(photo) ? null : photo.Trim();

    public Result<string> Normalize(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return Result<string>.Failure("A photo is required.");

        var raw = base64.Trim();
        string? declaredType = null;
        var payload = raw;

        if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var comma = raw.IndexOf(',');
            if (comma < 0)
                return Result<string>.Failure("Photo format is invalid.");

            var header = raw[..comma];
            payload = raw[(comma + 1)..];

            var typeStart = header.IndexOf(':');
            var typeEnd = header.IndexOf(';');
            if (typeStart < 0 || typeEnd < 0 || typeEnd <= typeStart)
                return Result<string>.Failure("Photo format is invalid.");

            declaredType = header[(typeStart + 1)..typeEnd].Trim();
            if (!header.Contains("base64", StringComparison.OrdinalIgnoreCase))
                return Result<string>.Failure("Photo must be base64.");
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload);
        }
        catch (FormatException)
        {
            return Result<string>.Failure("Photo must be a valid base64 image.");
        }

        if (bytes.Length == 0)
            return Result<string>.Failure("A photo is required.");

        if (bytes.Length > MaxBytes)
            return Result<string>.Failure("Image exceeds the 5 MB limit.");

        var detectedType = DetectContentType(bytes);
        if (detectedType is null)
            return Result<string>.Failure("Only JPG, PNG, and WebP images are allowed.");

        if (declaredType is not null
            && !declaredType.Equals(detectedType, StringComparison.OrdinalIgnoreCase)
            && !(declaredType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)
                && detectedType == "image/jpeg"))
        {
            return Result<string>.Failure("Only JPG, PNG, and WebP images are allowed.");
        }

        return Result<string>.Success($"data:{detectedType};base64,{Convert.ToBase64String(bytes)}");
    }

    private static string? DetectContentType(byte[] bytes)
    {
        if (Matches(bytes, Signatures["image/jpeg"][0]))
            return "image/jpeg";

        if (Matches(bytes, Signatures["image/png"][0]))
            return "image/png";

        if (bytes.Length >= 12
            && Matches(bytes, Signatures["image/webp"][0])
            && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
        {
            return "image/webp";
        }

        return null;
    }

    private static bool Matches(byte[] bytes, byte[] signature)
    {
        if (bytes.Length < signature.Length)
            return false;

        for (var i = 0; i < signature.Length; i++)
        {
            if (bytes[i] != signature[i])
                return false;
        }

        return true;
    }
}
