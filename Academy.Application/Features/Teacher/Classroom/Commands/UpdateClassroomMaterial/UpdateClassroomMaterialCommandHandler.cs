using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomMaterial;

public sealed class UpdateClassroomMaterialCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateClassroomMaterialCommand, Result<ClassroomMaterialDto>>
{
    public async Task<Result<ClassroomMaterialDto>> Handle(
        UpdateClassroomMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<ClassroomMaterialDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<ClassroomMaterialDto>.NotFound("الحصة غير موجودة.");

        var material = await dbContext.LessonSessionMaterials
            .AsTracking()
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(
                x => x.Id == request.MaterialId && x.LessonGroupSessionId == request.SessionId,
                cancellationToken);

        if (material is null)
            return Result<ClassroomMaterialDto>.NotFound("المادة غير موجودة.");

        if (request.Title is not null)
            material.Title = request.Title.Trim();

        if (request.Description is not null)
            material.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

        if (request.MaterialType.HasValue)
        {
            if (ClassroomMappings.IsUploadMaterialType(request.MaterialType.Value)
                && string.IsNullOrWhiteSpace(material.StoredFilePath)
                && !ClassroomMappings.IsUploadMaterialType(material.MaterialType))
            {
                return Result<ClassroomMaterialDto>.Failure(
                    "Cannot change material type to File or Recording without an uploaded file.");
            }

            material.MaterialType = request.MaterialType.Value;
        }

        if (request.ExternalUrl is not null)
        {
            material.ExternalUrl = string.IsNullOrWhiteSpace(request.ExternalUrl)
                ? null
                : request.ExternalUrl.Trim();
        }
        else if (request.MaterialType == ClassroomMaterialType.Link
                 && string.IsNullOrWhiteSpace(material.ExternalUrl))
        {
            return Result<ClassroomMaterialDto>.Failure(
                "A valid external URL is required for Link materials.");
        }

        if (request.Body is not null)
            material.Body = string.IsNullOrWhiteSpace(request.Body) ? null : request.Body.Trim();

        if (request.SortOrder.HasValue)
            material.SortOrder = request.SortOrder.Value;

        material.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<ClassroomMaterialDto>.Success(ClassroomMappings.ToMaterialDto(material));
    }
}
