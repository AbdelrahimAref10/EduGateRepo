using Academy.Domain.Enums;
using FluentValidation;

namespace Academy.Application.Features.Teacher.Lessons.Commands.CreateLesson;

public sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.EducationStageId).GreaterThan(0);
        RuleFor(x => x.EducationYearId).GreaterThan(0);
        RuleFor(x => x.EducationSubjectId).GreaterThan(0);
        RuleFor(x => x.AreaId).GreaterThan(0);

        RuleFor(x => x.BillingType)
            .IsInEnum();

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.SessionPrice)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.BillingType == BillingType.PerSession)
            .WithMessage("Session price is required when billing is per session.");

        RuleFor(x => x.MonthlyPrice)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.BillingType == BillingType.Monthly)
            .WithMessage("Monthly price is required when billing is monthly.");
    }
}
