using Academy.Application.Common.Models;

namespace Academy.Application.Contracts.Images;

public interface IImageService
{
    Result<string> Normalize(string? base64);

    string? Display(string? photo);
}
