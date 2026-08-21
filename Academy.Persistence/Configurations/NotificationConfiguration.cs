using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TitleEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BodyAr)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.BodyEn)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.UserTarget)
            .WithMany()
            .HasForeignKey(x => x.UserTargetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
