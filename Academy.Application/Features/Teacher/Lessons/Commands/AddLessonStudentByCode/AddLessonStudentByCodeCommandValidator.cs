using FluentValidation;

namespace Academy.Application.Features.Teacher.Lessons.Commands.AddLessonStudentByCode;

public sealed class AddLessonStudentByCodeCommandValidator : AbstractValidator<AddLessonStudentByCodeCommand>
{
    public AddLessonStudentByCodeCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);

        RuleFor(x => x.StudentCode)
            .NotEmpty()
            .MaximumLength(32);
    }
}
