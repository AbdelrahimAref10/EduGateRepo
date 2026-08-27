using FluentValidation;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterLessons;

public sealed class GetTeacherBillingFilterLessonsQueryValidator
    : AbstractValidator<GetTeacherBillingFilterLessonsQuery>
{
    public GetTeacherBillingFilterLessonsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.EducationStageId).GreaterThan(0);
    }
}
