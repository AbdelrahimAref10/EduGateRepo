using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.CreateClassroomMaterial;

public sealed record CreateClassroomMaterialCommand(
    int UserId,
    int SessionId,
    string Title,
    string? Description,
    ClassroomMaterialType MaterialType,
    string? ExternalUrl,
    string? Body,
    int? SortOrder) : IRequest<Result<ClassroomMaterialDto>>;
