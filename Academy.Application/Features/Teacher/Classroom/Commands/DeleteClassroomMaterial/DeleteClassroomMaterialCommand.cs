using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.DeleteClassroomMaterial;

public sealed record DeleteClassroomMaterialCommand(
    int UserId,
    int SessionId,
    int MaterialId) : IRequest<Result>;
