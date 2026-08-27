using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterStages;

public sealed class GetTeacherBillingFilterStagesQueryValidator
    : AbstractValidator<GetTeacherBillingFilterStagesQuery>
{
    public GetTeacherBillingFilterStagesQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
    }
}
