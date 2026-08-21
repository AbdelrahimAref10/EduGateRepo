using FluentValidation;

namespace Academy.Application.Features.Teacher.Lessons.Commands.CreateLessonGroup;

public sealed class CreateLessonGroupCommandValidator : AbstractValidator<CreateLessonGroupCommand>
{
    public CreateLessonGroupCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Dates)
            .NotEmpty()
            .WithMessage("أضف يوماً واحداً على الأقل للجدول.");

        RuleForEach(x => x.Dates).ChildRules(date =>
        {
            date.RuleFor(d => d.DayOfWeek).IsInEnum();
        });

        RuleFor(x => x.PeriodStartDate).NotEmpty();
        RuleFor(x => x.PeriodEndDate).NotEmpty();

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);

        RuleFor(x => x.AreaId)
            .GreaterThan(0)
            .When(x => x.AreaId.HasValue);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0)
            .When(x => x.MaxCapacity.HasValue);
    }
}
