using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Contracts.Storage;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherClassroomMaterialFile;

public sealed class GetTeacherClassroomMaterialFileQueryHandler(
    IApplicationDbContext dbContext,
    IClassroomFileStorage fileStorage)
    : IRequestHandler<GetTeacherClassroomMaterialFileQuery, Result<ClassroomFileDownloadDto>>
{
    public async Task<Result<ClassroomFileDownloadDto>> Handle(
        GetTeacherClassroomMaterialFileQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<ClassroomFileDownloadDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<ClassroomFileDownloadDto>.NotFound("الحصة غير موجودة.");

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
