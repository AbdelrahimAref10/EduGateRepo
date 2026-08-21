using FluentValidation;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomInfo;

public sealed class UpdateClassroomInfoCommandValidator : AbstractValidator<UpdateClassroomInfoCommand>
{
    public UpdateClassroomInfoCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);

        RuleFor(x => x.Topic)
            .MaximumLength(200)
            .When(x => x.Topic is not null);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);
    }
}
