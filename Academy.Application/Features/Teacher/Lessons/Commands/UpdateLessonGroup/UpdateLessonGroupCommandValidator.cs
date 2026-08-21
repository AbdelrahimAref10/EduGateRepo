using FluentValidation;

namespace Academy.Application.Features.Teacher.Lessons.Commands.UpdateLessonGroup;

public sealed class UpdateLessonGroupCommandValidator : AbstractValidator<UpdateLessonGroupCommand>
{
    public UpdateLessonGroupCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
        RuleFor(x => x.GroupId).GreaterThan(0);

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
        RuleFor(x => x.AreaId).GreaterThan(0);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => x.Notes is not null);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0)
            .When(x => x.MaxCapacity.HasValue);
    }
}
