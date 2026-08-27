using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterSessions;

public sealed class GetTeacherBillingFilterSessionsQueryValidator
    : AbstractValidator<GetTeacherBillingFilterSessionsQuery>
{
    public GetTeacherBillingFilterSessionsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.GroupId).GreaterThan(0);
    }
}
