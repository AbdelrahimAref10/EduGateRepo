using FluentValidation;

namespace Academy.Application.Features.Student.Classroom.Commands.SubmitStudentSessionExam;

public sealed class SubmitStudentSessionExamCommandValidator
    : AbstractValidator<SubmitStudentSessionExamCommand>
{
    public SubmitStudentSessionExamCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
    }
}
