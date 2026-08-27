using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherStudentOutstanding;

public sealed class GetTeacherStudentOutstandingQueryValidator
    : AbstractValidator<GetTeacherStudentOutstandingQuery>
{
    public GetTeacherStudentOutstandingQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);
    }
}
