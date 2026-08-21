using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}
