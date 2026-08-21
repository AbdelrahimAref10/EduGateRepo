using FluentValidation;

namespace Academy.Application.Features.Teacher.Lessons.Commands.AddGroupMemberByCode;

public sealed class AddGroupMemberByCodeCommandValidator : AbstractValidator<AddGroupMemberByCodeCommand>
{
    public AddGroupMemberByCodeCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
        RuleFor(x => x.GroupId).GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.StudentId is > 0 || !string.IsNullOrWhiteSpace(x.StudentCode))
            .WithMessage("Provide StudentId or StudentCode.");

        RuleFor(x => x.StudentCode)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.StudentCode));
    }
}
