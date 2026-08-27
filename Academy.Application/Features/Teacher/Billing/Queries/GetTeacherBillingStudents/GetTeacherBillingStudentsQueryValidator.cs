using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingStudents;

public sealed class GetTeacherBillingStudentsQueryValidator : AbstractValidator<GetTeacherBillingStudentsQuery>
{
    public GetTeacherBillingStudentsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
