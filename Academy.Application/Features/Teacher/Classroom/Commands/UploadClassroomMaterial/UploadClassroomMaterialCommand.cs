using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UploadClassroomMaterial;

public sealed record UploadClassroomMaterialCommand(
    int UserId,
    int SessionId,
    string Title,
    string? Description,
    ClassroomMaterialType MaterialType,
    Stream FileStream,
    string FileName,
    string ContentType,
    int? SortOrder) : IRequest<Result<ClassroomMaterialDto>>;
