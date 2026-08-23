using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonSessionStudentDetailConfiguration
    : IEntityTypeConfiguration<LessonSessionStudentDetail>
{
    public void Configure(EntityTypeBuilder<LessonSessionStudentDetail> builder)
    {
        builder.ToTable("LessonSessionStudentDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsPresent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.TeacherNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.LessonGroupSession)
            .WithMany(x => x.StudentDetails)
            .HasForeignKey(x => x.LessonGroupSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LessonGroupSessionId, x.StudentId })
            .IsUnique();
    }
}
