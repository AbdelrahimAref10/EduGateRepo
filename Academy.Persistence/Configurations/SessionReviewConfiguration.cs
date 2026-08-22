using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class SessionReviewConfiguration : IEntityTypeConfiguration<SessionReview>
{
    public void Configure(EntityTypeBuilder<SessionReview> builder)
    {
        builder.ToTable("SessionReviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.LessonGroupSession)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.LessonGroupSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Teacher)
            .WithMany(x => x.SessionReviews)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.SessionReviews)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LessonGroupSessionId, x.StudentId })
            .IsUnique();

        builder.HasIndex(x => new { x.TeacherId, x.CreatedAtUtc });
    }
}
