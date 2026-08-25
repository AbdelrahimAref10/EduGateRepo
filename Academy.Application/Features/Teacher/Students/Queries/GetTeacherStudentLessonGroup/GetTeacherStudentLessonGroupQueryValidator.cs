using FluentValidation;

namespace Academy.Application.Features.Teacher.Students.Queries.GetTeacherStudentLessonGroup;

public sealed class GetTeacherStudentLessonGroupQueryValidator
    : AbstractValidator<GetTeacherStudentLessonGroupQuery>
{
    public GetTeacherStudentLessonGroupQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
    }
}
