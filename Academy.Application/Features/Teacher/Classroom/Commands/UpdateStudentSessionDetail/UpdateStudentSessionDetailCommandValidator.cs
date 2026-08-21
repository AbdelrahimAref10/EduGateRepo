using FluentValidation;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateStudentSessionDetail;

public sealed class UpdateStudentSessionDetailCommandValidator
    : AbstractValidator<UpdateStudentSessionDetailCommand>
{
    public UpdateStudentSessionDetailCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);

        RuleFor(x => x.TeacherNotes)
            .MaximumLength(1000)
            .When(x => x.TeacherNotes is not null);
    }
}
