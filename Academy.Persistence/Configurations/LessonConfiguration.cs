using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BillingType)
            .IsRequired();

        builder.Property(x => x.SessionPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.MonthlyPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.ChargeAbsentSessions)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.StartedAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Teacher)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Area)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EducationType)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.EducationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EducationStage)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.EducationStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EducationYear)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.EducationYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EducationSubject)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.EducationSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TeacherId);
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.AreaId);
        builder.HasIndex(x => x.EducationTypeId);
        builder.HasIndex(x => x.EducationStageId);
        builder.HasIndex(x => x.EducationYearId);
        builder.HasIndex(x => x.EducationSubjectId);

        builder.Property(x => x.RatingAverage)
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(x => x.RatingCount)
            .IsRequired();
    }
}
