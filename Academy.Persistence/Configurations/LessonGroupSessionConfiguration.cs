using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonGroupSessionConfiguration : IEntityTypeConfiguration<LessonGroupSession>
{
    public void Configure(EntityTypeBuilder<LessonGroupSession> builder)
    {
        builder.ToTable("LessonGroupSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.StartTime)
            .IsRequired()
            .HasColumnType("time");

        builder.Property(x => x.Topic)
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.LessonGroup)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.LessonGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.LessonGroupId);
        builder.HasIndex(x => new { x.LessonGroupId, x.SessionDate, x.StartTime })
            .IsUnique();

        builder.Property(x => x.RatingAverage)
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(x => x.RatingCount)
            .IsRequired();
    }
}
