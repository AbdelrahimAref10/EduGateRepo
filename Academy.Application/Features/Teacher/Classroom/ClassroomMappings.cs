using Academy.Domain.Entities;
using Academy.Domain.Enums;
using Academy.Application.Features.Teacher.Classroom.Dtos;

namespace Academy.Application.Features.Teacher.Classroom;

internal static class ClassroomMappings
{
    public static ClassroomMaterialDto ToMaterialDto(LessonSessionMaterial material) =>
        new()
        {
            Id = material.Id,
            Title = material.Title,
            Description = material.Description,
            MaterialType = (int)material.MaterialType,
            MaterialTypeName = material.MaterialType.ToString(),
            ExternalUrl = material.ExternalUrl,
            OriginalFileName = material.OriginalFileName,
            ContentType = material.ContentType,
            FileSizeBytes = material.FileSizeBytes,
            Body = material.Body,
            SortOrder = material.SortOrder,
            HasFile = !string.IsNullOrWhiteSpace(material.StoredFilePath),
            CreatedAtUtc = material.CreatedAtUtc,
            UpdatedAtUtc = material.UpdatedAtUtc,
            CreatedByName = material.CreatedByUser?.FullName ?? string.Empty
        };

    public static ClassroomStudentDetailDto ToStudentDetailDto(LessonSessionStudentDetail detail) =>
        new()
        {
            Id = detail.Id,
            StudentId = detail.StudentId,
            StudentName = detail.Student.User.FullName,
            StudentCode = detail.Student.StudentCode,
            IsPresent = detail.IsPresent,
            IsPaid = detail.IsPaid,
            TeacherNotes = detail.TeacherNotes,
            UpdatedAtUtc = detail.UpdatedAtUtc
        };

    public static TeacherClassroomDto ToTeacherClassroomDto(
        LessonGroupSession session,
        IReadOnlyList<ClassroomStudentDetailDto> students,
        IReadOnlyList<ClassroomMaterialDto> materials)
    {
        var lesson = session.LessonGroup.Lesson;
        var teacherUser = lesson.Teacher.User;

        return new TeacherClassroomDto
        {
            SessionId = session.Id,
            LessonId = lesson.Id,
            LessonGroupId = session.LessonGroupId,
            GroupName = session.LessonGroup.Name,
            Subject = lesson.Subject,
            SessionDate = session.SessionDate,
            StartTime = session.StartTime,
            Topic = session.Topic,
            Description = session.Description,
            HasStarted = session.StartedAtUtc.HasValue,
            HasEnded = session.EndedAtUtc.HasValue,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = session.EndedAtUtc,
            TeacherName = teacherUser.FullName,
            Students = students,
            Materials = materials
        };
    }

    public static bool IsUploadMaterialType(ClassroomMaterialType type) =>
        type is ClassroomMaterialType.File or ClassroomMaterialType.Recording;
}
