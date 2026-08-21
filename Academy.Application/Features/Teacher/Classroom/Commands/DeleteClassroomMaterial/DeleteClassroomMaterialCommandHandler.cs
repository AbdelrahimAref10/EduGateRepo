using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Contracts.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.DeleteClassroomMaterial;

public sealed class DeleteClassroomMaterialCommandHandler(
    IApplicationDbContext dbContext,
    IClassroomFileStorage fileStorage)
    : IRequestHandler<DeleteClassroomMaterialCommand, Result>
{
    public async Task<Result> Handle(
        DeleteClassroomMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result.NotFound("الحصة غير موجودة.");

        var material = await dbContext.LessonSessionMaterials
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.MaterialId && x.LessonGroupSessionId == request.SessionId,
                cancellationToken);

        if (material is null)
            return Result.NotFound("المادة غير موجودة.");

        var storedPath = material.StoredFilePath;

        dbContext.LessonSessionMaterials.Remove(material);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(storedPath))
            await fileStorage.DeleteAsync(storedPath, cancellationToken);

        return Result.Success();
    }
}
