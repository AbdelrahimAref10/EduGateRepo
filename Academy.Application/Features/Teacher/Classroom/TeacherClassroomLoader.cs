using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom;

internal static class TeacherClassroomLoader
{
    public static async Task<LessonGroupSession?> LoadOwnedSessionAsync(
        IApplicationDbContext dbContext,
        int teacherId,
        int sessionId,
        CancellationToken cancellationToken,
        bool asTracking = false)
    {
        var query = asTracking
            ? dbContext.LessonGroupSessions.AsTracking()
            : dbContext.LessonGroupSessions.AsNoTracking();

        return await query
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
                    .ThenInclude(x => x.Teacher)
                        .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Id == sessionId && x.LessonGroup.Lesson.TeacherId == teacherId,
                cancellationToken);
    }

    public static async Task<TeacherClassroomDto> BuildDtoAsync(
        IApplicationDbContext dbContext,
        LessonGroupSession session,
        CancellationToken cancellationToken)
    {
        await ClassroomSeeding.EnsureStudentDetailsAsync(dbContext, session, cancellationToken);

        var members = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .OrderBy(x => x.AddedAtUtc)
            .ToListAsync(cancellationToken);

        var details = await dbContext.LessonSessionStudentDetails
            .AsTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Where(x => x.LessonGroupSessionId == session.Id)
            .ToListAsync(cancellationToken);

        var detailByStudentId = details.ToDictionary(x => x.StudentId);
        var now = DateTime.UtcNow;
        var missingCreated = false;

        foreach (var member in members)
        {
            if (detailByStudentId.ContainsKey(member.StudentId))
                continue;

            var created = new LessonSessionStudentDetail
            {
                LessonGroupSessionId = session.Id,
                StudentId = member.StudentId,
                Student = member.Student,
                IsPresent = false,
                IsPaid = false,
                CreatedAtUtc = now
            };
            dbContext.LessonSessionStudentDetails.Add(created);
            detailByStudentId[member.StudentId] = created;
            missingCreated = true;
        }

        if (missingCreated)
            await dbContext.SaveChangesAsync(cancellationToken);

        var students = members
            .Select(m => ClassroomMappings.ToStudentDetailDto(detailByStudentId[m.StudentId]))
            .ToList();

        var materials = await dbContext.LessonSessionMaterials
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Where(x => x.LessonGroupSessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        // Ensure session navigations needed for DTO are present
        if (session.LessonGroup.Lesson.Teacher.User is null)
        {
            session = (await dbContext.LessonGroupSessions
                .AsNoTracking()
                .Include(x => x.LessonGroup)
                    .ThenInclude(x => x.Lesson)
                        .ThenInclude(x => x.Teacher)
                            .ThenInclude(x => x.User)
                .FirstAsync(x => x.Id == session.Id, cancellationToken))!;
        }

        var sessionNumber = await SessionNumbers.RankAsync(dbContext, session, cancellationToken);

        return ClassroomMappings.ToTeacherClassroomDto(
            session,
            sessionNumber,
            students,
            materials.Select(ClassroomMappings.ToMaterialDto).ToList());
    }
}
