using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class NotificationDetailConfiguration : IEntityTypeConfiguration<NotificationDetail>
{
    public void Configure(EntityTypeBuilder<NotificationDetail> builder)
    {
        builder.ToTable("NotificationDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Notification)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.NotificationDetails)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.NotificationId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAtUtc });
    }
}
