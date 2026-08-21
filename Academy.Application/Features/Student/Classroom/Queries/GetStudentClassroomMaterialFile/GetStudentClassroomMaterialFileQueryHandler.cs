using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Contracts.Storage;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentClassroomMaterialFile;

public sealed class GetStudentClassroomMaterialFileQueryHandler(
    IApplicationDbContext dbContext,
    IClassroomFileStorage fileStorage)
    : IRequestHandler<GetStudentClassroomMaterialFileQuery, Result<ClassroomFileDownloadDto>>
{
    public async Task<Result<ClassroomFileDownloadDto>> Handle(
        GetStudentClassroomMaterialFileQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<ClassroomFileDownloadDto>.NotFound("Student profile was not found.");

        var session = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SessionId, cancellationToken);

        if (session is null)
            return Result<ClassroomFileDownloadDto>.NotFound("الحصة غير موجودة.");

        var isMember = await dbContext.LessonGroupMembers
            .AnyAsync(
                x => x.LessonGroupId == session.LessonGroupId && x.StudentId == student.Id,
                cancellationToken);

        if (!isMember)
            return Result<ClassroomFileDownloadDto>.NotFound("الحصة غير موجودة.");

        if (!session.StartedAtUtc.HasValue)
            return Result<ClassroomFileDownloadDto>.Conflict("لم يتم بدء الحصة بعد.");

        var material = await dbContext.LessonSessionMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.MaterialId && x.LessonGroupSessionId == request.SessionId,
                cancellationToken);

        if (material is null || string.IsNullOrWhiteSpace(material.StoredFilePath))
            return Result<ClassroomFileDownloadDto>.NotFound("الملف غير موجود.");

        var content = await fileStorage.OpenReadAsync(material.StoredFilePath, cancellationToken);
        if (content is null)
            return Result<ClassroomFileDownloadDto>.NotFound("الملف غير موجود.");

        return Result<ClassroomFileDownloadDto>.Success(new ClassroomFileDownloadDto
        {
            Stream = content.Stream,
            ContentType = string.IsNullOrWhiteSpace(material.ContentType)
                ? content.ContentType
                : material.ContentType,
            FileName = string.IsNullOrWhiteSpace(material.OriginalFileName)
                ? content.FileName
                : material.OriginalFileName
        });
    }
}
