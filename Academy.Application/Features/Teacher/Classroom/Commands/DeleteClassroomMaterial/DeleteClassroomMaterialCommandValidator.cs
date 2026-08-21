using FluentValidation;

namespace Academy.Application.Features.Teacher.Classroom.Commands.DeleteClassroomMaterial;

public sealed class DeleteClassroomMaterialCommandValidator
    : AbstractValidator<DeleteClassroomMaterialCommand>
{
    public DeleteClassroomMaterialCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.MaterialId).GreaterThan(0);
    }
}
