using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonReviewConfiguration : IEntityTypeConfiguration<LessonReview>
{
    public void Configure(EntityTypeBuilder<LessonReview> builder)
    {
        builder.ToTable("LessonReviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Teacher)
            .WithMany(x => x.LessonReviews)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.LessonReviews)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LessonId, x.StudentId })
            .IsUnique();

        builder.HasIndex(x => new { x.TeacherId, x.CreatedAtUtc });
    }
}
