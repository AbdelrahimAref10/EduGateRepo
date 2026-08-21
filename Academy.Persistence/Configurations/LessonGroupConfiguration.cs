using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonGroupConfiguration : IEntityTypeConfiguration<LessonGroup>
{
    public void Configure(EntityTypeBuilder<LessonGroup> builder)
    {
        builder.ToTable("LessonGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.PeriodStartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.PeriodEndDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Groups)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Area)
            .WithMany(x => x.LessonGroups)
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.LessonId);
        builder.HasIndex(x => x.AreaId);
    }
}
