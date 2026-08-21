using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomMaterial;

public sealed record UpdateClassroomMaterialCommand(
    int UserId,
    int SessionId,
    int MaterialId,
    string? Title,
    string? Description,
    ClassroomMaterialType? MaterialType,
    string? ExternalUrl,
    string? Body,
    int? SortOrder) : IRequest<Result<ClassroomMaterialDto>>;
