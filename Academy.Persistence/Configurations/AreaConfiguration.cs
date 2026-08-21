using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.CityId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.City)
            .WithMany(x => x.Areas)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CityId, x.NameEn });
    }
}
