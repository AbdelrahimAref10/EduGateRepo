using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.GovernorateId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.Governorate)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.GovernorateId, x.NameEn });
    }
}
