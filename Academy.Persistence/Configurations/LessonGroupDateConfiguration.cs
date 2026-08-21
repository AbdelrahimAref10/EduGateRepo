using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonGroupDateConfiguration : IEntityTypeConfiguration<LessonGroupDate>
{
    public void Configure(EntityTypeBuilder<LessonGroupDate> builder)
    {
        builder.ToTable("LessonGroupDates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .IsRequired()
            .HasColumnType("time");

        builder.HasOne(x => x.LessonGroup)
            .WithMany(x => x.Dates)
            .HasForeignKey(x => x.LessonGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LessonGroupId, x.DayOfWeek })
            .IsUnique();

        builder.HasIndex(x => x.LessonGroupId);
    }
}
