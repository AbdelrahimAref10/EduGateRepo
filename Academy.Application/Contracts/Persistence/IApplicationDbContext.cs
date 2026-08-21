using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Contracts.Persistence;

public interface IApplicationDbContext
{
    DbSet<Student> Students { get; }

    DbSet<Teacher> Teachers { get; }

    DbSet<SuperAdmin> SuperAdmins { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Country> Countries { get; }

    DbSet<Governorate> Governorates { get; }

    DbSet<City> Cities { get; }

    DbSet<Area> Areas { get; }

    DbSet<EducationType> EducationTypes { get; }

    DbSet<EducationStage> EducationStages { get; }

    DbSet<EducationYear> EducationYears { get; }

    DbSet<EducationSubject> EducationSubjects { get; }

    DbSet<Lesson> Lessons { get; }

    DbSet<LessonBooking> LessonBookings { get; }

    DbSet<LessonGroup> LessonGroups { get; }

    DbSet<LessonGroupDate> LessonGroupDates { get; }

    DbSet<LessonGroupMember> LessonGroupMembers { get; }

    DbSet<LessonGroupSession> LessonGroupSessions { get; }

    DbSet<LessonSessionStudentDetail> LessonSessionStudentDetails { get; }

    DbSet<LessonSessionMaterial> LessonSessionMaterials { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<NotificationDetail> NotificationDetails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
