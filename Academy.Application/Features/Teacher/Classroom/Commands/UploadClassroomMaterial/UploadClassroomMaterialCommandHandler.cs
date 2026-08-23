using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Contracts.Storage;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UploadClassroomMaterial;

public sealed class UploadClassroomMaterialCommandHandler(
    IApplicationDbContext dbContext,
    IClassroomFileStorage fileStorage,
    INotificationService notificationService)
    : IRequestHandler<UploadClassroomMaterialCommand, Result<ClassroomMaterialDto>>
{
    public async Task<Result<ClassroomMaterialDto>> Handle(
        UploadClassroomMaterialCommand request,
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

        StoredClassroomFile stored;
        try
        {
            stored = await fileStorage.SaveAsync(
                request.SessionId,
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result<ClassroomMaterialDto>.Failure(ex.Message);
        }

        var sortOrder = request.SortOrder
            ?? await NextSortOrderAsync(dbContext, request.SessionId, cancellationToken);

        var material = new LessonSessionMaterial
        {
            LessonGroupSessionId = request.SessionId,
            Title = string.IsNullOrWhiteSpace(request.Title)
                ? Path.GetFileNameWithoutExtension(request.FileName)
                : request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            MaterialType = ClassroomMaterialType.File,
            StoredFilePath = stored.RelativePath,
            OriginalFileName = stored.OriginalFileName,
            ContentType = stored.ContentType,
            FileSizeBytes = stored.SizeBytes,
            SortOrder = sortOrder,
            CreatedByUserId = request.UserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.LessonSessionMaterials.Add(material);
        await dbContext.SaveChangesAsync(cancellationToken);

        material = await dbContext.LessonSessionMaterials
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .FirstAsync(x => x.Id == material.Id, cancellationToken);

        await ClassroomMaterialNotifier.NotifyGroupAsync(
            dbContext,
            notificationService,
            session,
            request.UserId,
            cancellationToken);

        return Result<ClassroomMaterialDto>.Success(ClassroomMappings.ToMaterialDto(material));
    }

    private static async Task<int> NextSortOrderAsync(
        IApplicationDbContext dbContext,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var max = await dbContext.LessonSessionMaterials
            .Where(x => x.LessonGroupSessionId == sessionId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);

        return (max ?? -1) + 1;
    }
}
