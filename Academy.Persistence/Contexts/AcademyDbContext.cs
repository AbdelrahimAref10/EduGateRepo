using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Academy.Persistence.Contexts;

public sealed class AcademyDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        int,
        ApplicationUserClaim,
        ApplicationUserRole,
        ApplicationUserLogin,
        ApplicationRoleClaim,
        ApplicationUserToken>,
      IApplicationDbContext
{
    public AcademyDbContext(DbContextOptions<AcademyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Governorate> Governorates => Set<Governorate>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<Area> Areas => Set<Area>();

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();

    public DbSet<EducationStage> EducationStages => Set<EducationStage>();

    public DbSet<EducationYear> EducationYears => Set<EducationYear>();

    public DbSet<EducationSubject> EducationSubjects => Set<EducationSubject>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<LessonBooking> LessonBookings => Set<LessonBooking>();

    public DbSet<LessonGroup> LessonGroups => Set<LessonGroup>();

    public DbSet<LessonGroupDate> LessonGroupDates => Set<LessonGroupDate>();

    public DbSet<LessonGroupMember> LessonGroupMembers => Set<LessonGroupMember>();

    public DbSet<LessonGroupSession> LessonGroupSessions => Set<LessonGroupSession>();

    public DbSet<LessonSessionStudentDetail> LessonSessionStudentDetails => Set<LessonSessionStudentDetail>();

    public DbSet<LessonSessionMaterial> LessonSessionMaterials => Set<LessonSessionMaterial>();

    public DbSet<Exam> Exams => Set<Exam>();

    public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();

    public DbSet<ExamQuestionOption> ExamQuestionOptions => Set<ExamQuestionOption>();

    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();

    public DbSet<ExamAttemptAnswer> ExamAttemptAnswers => Set<ExamAttemptAnswer>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationDetail> NotificationDetails => Set<NotificationDetail>();

    public DbSet<TeacherReview> TeacherReviews => Set<TeacherReview>();

    public DbSet<LessonReview> LessonReviews => Set<LessonReview>();

    public DbSet<SessionReview> SessionReviews => Set<SessionReview>();

    public DbSet<Charge> Charges => Set<Charge>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();

    public DbSet<ParentChildLink> ParentChildLinks => Set<ParentChildLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AcademyDbContext).Assembly);
    }
}
