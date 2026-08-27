using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Method)
            .IsRequired();

        builder.Property(x => x.PaidAtUtc)
            .IsRequired();

        builder.Property(x => x.ReceiptNumber)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Lesson)
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TeacherId, x.ReceiptNumber })
            .IsUnique();

        builder.HasIndex(x => new { x.LessonId, x.StudentId });
        builder.HasIndex(x => new { x.TeacherId, x.PaidAtUtc });
        builder.HasIndex(x => x.PaidAtUtc);
    }
}
