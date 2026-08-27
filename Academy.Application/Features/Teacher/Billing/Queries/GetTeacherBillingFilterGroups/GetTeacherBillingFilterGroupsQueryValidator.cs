using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterGroups;

public sealed class GetTeacherBillingFilterGroupsQueryValidator
    : AbstractValidator<GetTeacherBillingFilterGroupsQuery>
{
    public GetTeacherBillingFilterGroupsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
    }
}
