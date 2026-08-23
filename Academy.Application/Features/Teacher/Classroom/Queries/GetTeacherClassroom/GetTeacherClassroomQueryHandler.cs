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
    private sealed class StudentRow
    {
        public int Id { get; init; }
        public int StudentId { get; init; }
        public int UserId { get; init; }
        public string StudentName { get; init; } = string.Empty;
        public string? Photo { get; init; }
        public string? StudentCode { get; init; }
        public bool IsPresent { get; init; }
        public string? TeacherNotes { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }

    public async Task<Result<TeacherClassroomDto>> Handle(
        GetTeacherClassroomQuery request,
        CancellationToken cancellationToken)
    {
        var sessionEntity = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.SessionId
                     && x.LessonGroup.Lesson.Teacher.UserId == request.UserId,
                cancellationToken);

        if (sessionEntity is null)
            return Result<TeacherClassroomDto>.NotFound("الحصة غير موجودة.");

        var lesson = sessionEntity.LessonGroup.Lesson;

        var session = await dbContext.LessonGroupSessions
            .Where(x => x.Id == request.SessionId)
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
                x.IsMakeup,
                TeacherName = x.LessonGroup.Lesson.Teacher.User.FullName,
                TeacherPhoto = x.LessonGroup.Lesson.Teacher.User.ProfilePhoto,
                SessionNumber = dbContext.LessonGroupSessions.Count(s =>
                    s.LessonGroupId == x.LessonGroupId
                    && (s.SessionDate < x.SessionDate
                        || (s.SessionDate == x.SessionDate && s.StartTime < x.StartTime)
                        || (s.SessionDate == x.SessionDate && s.StartTime == x.StartTime && s.Id <= x.Id)))
            })
            .FirstAsync(cancellationToken);

        if (!session.IsMakeup)
        {
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
                        CreatedAtUtc = now
                    });
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        List<StudentRow> rows;
        if (session.IsMakeup)
        {
            rows = await dbContext.LessonSessionStudentDetails
                .Where(x => x.LessonGroupSessionId == session.Id)
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new StudentRow
                {
                    Id = x.Id,
                    StudentId = x.StudentId,
                    UserId = x.Student.UserId,
                    StudentName = x.Student.User.FullName,
                    Photo = x.Student.User.ProfilePhoto,
                    StudentCode = x.Student.StudentCode,
                    IsPresent = x.IsPresent,
                    TeacherNotes = x.TeacherNotes,
                    UpdatedAtUtc = x.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }
        else
        {
            rows = await (
                from member in dbContext.LessonGroupMembers
                join detail in dbContext.LessonSessionStudentDetails
                    on new { member.StudentId, SessionId = session.Id }
                    equals new { detail.StudentId, SessionId = detail.LessonGroupSessionId }
                where member.LessonGroupId == session.LessonGroupId
                orderby member.AddedAtUtc
                select new StudentRow
                {
                    Id = detail.Id,
                    StudentId = detail.StudentId,
                    UserId = detail.Student.UserId,
                    StudentName = detail.Student.User.FullName,
                    Photo = detail.Student.User.ProfilePhoto,
                    StudentCode = detail.Student.StudentCode,
                    IsPresent = detail.IsPresent,
                    TeacherNotes = detail.TeacherNotes,
                    UpdatedAtUtc = detail.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        var students = new List<ClassroomStudentDetailDto>();
        foreach (var x in rows)
        {
            var charges = await ClassroomChargeQuery.ForStudentAsync(
                dbContext,
                lesson,
                sessionEntity,
                x.StudentId,
                cancellationToken);
            var (outstanding, status) = Charge.Summarize(charges);

            students.Add(new ClassroomStudentDetailDto
            {
                Id = x.Id,
                StudentId = x.StudentId,
                UserId = x.UserId,
                StudentName = x.StudentName,
                PhotoUrl = ImageService.DisplayValue(x.Photo),
                StudentCode = x.StudentCode,
                IsPresent = x.IsPresent,
                OutstandingAmount = outstanding,
                BillingStatus = status,
                TeacherNotes = x.TeacherNotes,
                UpdatedAtUtc = x.UpdatedAtUtc
            });
        }

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
            Students = students,
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
