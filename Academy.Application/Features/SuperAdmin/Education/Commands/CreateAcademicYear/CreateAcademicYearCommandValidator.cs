using FluentValidation;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateAcademicYear;

public sealed class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    public CreateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
