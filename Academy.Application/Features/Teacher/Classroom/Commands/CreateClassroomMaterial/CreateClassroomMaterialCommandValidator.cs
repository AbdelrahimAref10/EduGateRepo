using Academy.Domain.Enums;
using FluentValidation;

namespace Academy.Application.Features.Teacher.Classroom.Commands.CreateClassroomMaterial;

public sealed class CreateClassroomMaterialCommandValidator
    : AbstractValidator<CreateClassroomMaterialCommand>
{
    public CreateClassroomMaterialCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.MaterialType).IsInEnum();

        RuleFor(x => x.MaterialType)
            .Must(t => t is not ClassroomMaterialType.File and not ClassroomMaterialType.Recording)
            .WithMessage("Use the upload endpoint for File or Recording materials.");

        RuleFor(x => x.ExternalUrl)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(BeValidAbsoluteUrl)
            .When(x => x.MaterialType == ClassroomMaterialType.Link)
            .WithMessage("A valid external URL is required for Link materials.");

        RuleFor(x => x.ExternalUrl)
            .MaximumLength(2000)
            .Must(url => string.IsNullOrWhiteSpace(url) || BeValidAbsoluteUrl(url))
            .When(x => x.MaterialType != ClassroomMaterialType.Link && x.ExternalUrl is not null)
            .WithMessage("External URL must be a valid absolute URL.");

        RuleFor(x => x.Body)
            .MaximumLength(20000)
            .When(x => x.Body is not null);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue);
    }

    private static bool BeValidAbsoluteUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
