using FluentValidation;

namespace Academy.Application.Features.Teacher.Students.Queries.GetTeacherStudentLessons;

public sealed class GetTeacherStudentLessonsQueryValidator : AbstractValidator<GetTeacherStudentLessonsQuery>
{
    public GetTeacherStudentLessonsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);
    }
}
