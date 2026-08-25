using FluentValidation;

namespace Academy.Application.Features.Teacher.Students.Queries.GetMyStudents;

public sealed class GetMyStudentsQueryValidator : AbstractValidator<GetMyStudentsQuery>
{
    public GetMyStudentsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
