using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class EducationStageConfiguration : IEntityTypeConfiguration<EducationStage>
{
    public void Configure(EntityTypeBuilder<EducationStage> builder)
    {
        builder.ToTable("EducationStages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.NameEn)
            .IsUnique();
    }
}
