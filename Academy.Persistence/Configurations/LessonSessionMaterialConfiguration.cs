using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonSessionMaterialConfiguration
    : IEntityTypeConfiguration<LessonSessionMaterial>
{
    public void Configure(EntityTypeBuilder<LessonSessionMaterial> builder)
    {
        builder.ToTable("LessonSessionMaterials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.MaterialType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ExternalUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.StoredFilePath)
            .HasMaxLength(500);

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(260);

        builder.Property(x => x.ContentType)
            .HasMaxLength(150);

        builder.Property(x => x.Body)
            .HasMaxLength(20000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.LessonGroupSession)
            .WithMany(x => x.Materials)
            .HasForeignKey(x => x.LessonGroupSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.LessonGroupSessionId);
        builder.HasIndex(x => new { x.LessonGroupSessionId, x.SortOrder });
    }
}
