using Academy.Domain.Enums;
using FluentValidation;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UploadClassroomMaterial;

public sealed class UploadClassroomMaterialCommandValidator
    : AbstractValidator<UploadClassroomMaterialCommand>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx"
    };

    public UploadClassroomMaterialCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("A file is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(260)
            .Must(HasAllowedExtension)
            .WithMessage("Only PDF, Word, and Excel files are allowed (.pdf, .doc, .docx, .xls, .xlsx).");

        RuleFor(x => x.ContentType)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.ContentType));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue);
    }

    private static bool HasAllowedExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var ext = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(ext) && AllowedExtensions.Contains(ext);
    }
}
