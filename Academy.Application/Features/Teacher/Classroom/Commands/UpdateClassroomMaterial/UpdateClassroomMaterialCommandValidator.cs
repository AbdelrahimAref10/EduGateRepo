using FluentValidation;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomMaterial;

public sealed class UpdateClassroomMaterialCommandValidator
    : AbstractValidator<UpdateClassroomMaterialCommand>
{
    public UpdateClassroomMaterialCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.MaterialId).GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.MaterialType)
            .IsInEnum()
            .When(x => x.MaterialType.HasValue);

        RuleFor(x => x.ExternalUrl)
            .MaximumLength(2000)
            .Must(url => string.IsNullOrWhiteSpace(url) || BeValidAbsoluteUrl(url))
            .When(x => x.ExternalUrl is not null)
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
