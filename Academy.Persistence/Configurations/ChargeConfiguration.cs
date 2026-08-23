using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.ToTable("Charges");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Settlement).IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AllocatedAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CycleStartDate)
            .HasColumnType("date");

        builder.Property(x => x.CycleEndDate)
            .HasColumnType("date");

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(x => x.Remaining);
        builder.Ignore(x => x.HasAllocations);
        builder.Ignore(x => x.CanBeRemoved);
        builder.Ignore(x => x.IsOpenForPayment);

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

        builder.HasOne(x => x.LessonGroup)
            .WithMany()
            .HasForeignKey(x => x.LessonGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LessonGroupSession)
            .WithMany()
            .HasForeignKey(x => x.LessonGroupSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentCharge)
            .WithMany(x => x.ChildCharges)
            .HasForeignKey(x => x.ParentChargeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LessonId, x.StudentId, x.Status });
        builder.HasIndex(x => new { x.TeacherId, x.StudentId });
        builder.HasIndex(x => x.LessonGroupSessionId);
        builder.HasIndex(x => new { x.LessonId, x.StudentId, x.Type, x.CycleStartDate });
    }
}
