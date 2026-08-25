using FluentValidation;

namespace Academy.Application.Features.Teacher.Students.Commands.TransferStudentGroup;

public sealed class TransferStudentGroupCommandValidator : AbstractValidator<TransferStudentGroupCommand>
{
    public TransferStudentGroupCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
        RuleFor(x => x.TargetGroupId).GreaterThan(0);
    }
}
