using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherClassroom;

public sealed class GetTeacherClassroomQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherClassroomQuery, Result<TeacherClassroomDto>>
{
    public async Task<Result<TeacherClassroomDto>> Handle(
        GetTeacherClassroomQuery request,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.LessonGroupSessions
            .Where(x =>
                x.Id == request.SessionId
                && x.LessonGroup.Lesson.Teacher.UserId == request.UserId)
            .Select(x => new
            {
                x.Id,
                LessonId = x.LessonGroup.LessonId,
                x.LessonGroupId,
                GroupName = x.LessonGroup.Name,
                Subject = x.LessonGroup.Lesson.Subject,
                x.SessionDate,
                x.StartTime,
                x.Topic,
                x.Description,
                x.StartedAtUtc,
                x.EndedAtUtc,
                TeacherName = x.LessonGroup.Lesson.Teacher.User.FullName,
                TeacherPhoto = x.LessonGroup.Lesson.Teacher.User.ProfilePhoto,
                SessionNumber = dbContext.LessonGroupSessions.Count(s =>
                    s.LessonGroupId == x.LessonGroupId
                    && (s.SessionDate < x.SessionDate
                        || (s.SessionDate == x.SessionDate && s.StartTime < x.StartTime)
                        || (s.SessionDate == x.SessionDate && s.StartTime == x.StartTime && s.Id <= x.Id)))
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            return Result<TeacherClassroomDto>.NotFound("الحصة غير موجودة.");

        var memberIds = await dbContext.LessonGroupMembers
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .Select(x => x.StudentId)
            .ToListAsync(cancellationToken);

        var existingIds = await dbContext.LessonSessionStudentDetails
            .Where(x => x.LessonGroupSessionId == session.Id)
            .Select(x => x.StudentId)
            .ToListAsync(cancellationToken);

        var missingIds = memberIds.Except(existingIds).ToList();
        if (missingIds.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var studentId in missingIds)
            {
                dbContext.LessonSessionStudentDetails.Add(new LessonSessionStudentDetail
                {
                    LessonGroupSessionId = session.Id,
                    StudentId = studentId,
                    IsPresent = false,
                    IsPaid = false,
                    CreatedAtUtc = now
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var studentRows = await (
            from member in dbContext.LessonGroupMembers
            join detail in dbContext.LessonSessionStudentDetails
                on new { member.StudentId, SessionId = session.Id }
                equals new { detail.StudentId, SessionId = detail.LessonGroupSessionId }
            where member.LessonGroupId == session.LessonGroupId
            orderby member.AddedAtUtc
            select new
            {
                detail.Id,
                detail.StudentId,
                UserId = detail.Student.UserId,
                StudentName = detail.Student.User.FullName,
                Photo = detail.Student.User.ProfilePhoto,
                detail.Student.StudentCode,
                detail.IsPresent,
                detail.IsPaid,
                detail.TeacherNotes,
                detail.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var materialRows = await dbContext.LessonSessionMaterials
            .Where(x => x.LessonGroupSessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                x.MaterialType,
                x.ExternalUrl,
                x.OriginalFileName,
                x.ContentType,
                x.FileSizeBytes,
                x.Body,
                x.SortOrder,
                HasFile = x.StoredFilePath != null && x.StoredFilePath != "",
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                CreatedByName = x.CreatedByUser.FullName
            })
            .ToListAsync(cancellationToken);

        return Result<TeacherClassroomDto>.Success(new TeacherClassroomDto
        {
            SessionId = session.Id,
            LessonId = session.LessonId,
            LessonGroupId = session.LessonGroupId,
            SessionNumber = session.SessionNumber,
            GroupName = session.GroupName,
            Subject = session.Subject,
            SessionDate = session.SessionDate,
            StartTime = session.StartTime,
            Topic = session.Topic,
            Description = session.Description,
            HasStarted = session.StartedAtUtc.HasValue,
            HasEnded = session.EndedAtUtc.HasValue,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = session.EndedAtUtc,
            TeacherName = session.TeacherName,
            TeacherPhotoUrl = ImageService.DisplayValue(session.TeacherPhoto),
            Students = studentRows
                .Select(x => new ClassroomStudentDetailDto
                {
                    Id = x.Id,
                    StudentId = x.StudentId,
                    UserId = x.UserId,
                    StudentName = x.StudentName,
                    PhotoUrl = ImageService.DisplayValue(x.Photo),
                    StudentCode = x.StudentCode,
                    IsPresent = x.IsPresent,
                    IsPaid = x.IsPaid,
                    TeacherNotes = x.TeacherNotes,
                    UpdatedAtUtc = x.UpdatedAtUtc
                })
                .ToList(),
            Materials = materialRows
                .Select(x => new ClassroomMaterialDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    MaterialType = (int)x.MaterialType,
                    MaterialTypeName = x.MaterialType.ToString(),
                    ExternalUrl = x.ExternalUrl,
                    OriginalFileName = x.OriginalFileName,
                    ContentType = x.ContentType,
                    FileSizeBytes = x.FileSizeBytes,
                    Body = x.Body,
                    SortOrder = x.SortOrder,
                    HasFile = x.HasFile,
                    CreatedAtUtc = x.CreatedAtUtc,
                    UpdatedAtUtc = x.UpdatedAtUtc,
                    CreatedByName = x.CreatedByName
                })
                .ToList()
        });
    }
}
