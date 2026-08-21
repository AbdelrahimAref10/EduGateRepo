using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class EducationYearConfiguration : IEntityTypeConfiguration<EducationYear>
{
    public void Configure(EntityTypeBuilder<EducationYear> builder)
    {
        builder.ToTable("EducationYears");

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

        builder.HasOne(x => x.EducationType)
            .WithMany(x => x.Years)
            .HasForeignKey(x => x.EducationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EducationTypeId, x.SortOrder });
        builder.HasIndex(x => new { x.EducationTypeId, x.NameEn });
    }
}
