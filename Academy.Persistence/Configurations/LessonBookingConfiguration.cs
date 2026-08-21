using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonBookingConfiguration : IEntityTypeConfiguration<LessonBooking>
{
    public void Configure(EntityTypeBuilder<LessonBooking> builder)
    {
        builder.ToTable("LessonBookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Teacher)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LessonId, x.StudentId })
            .IsUnique();

        builder.HasIndex(x => x.TeacherId);
        builder.HasIndex(x => x.Status);
    }
}
