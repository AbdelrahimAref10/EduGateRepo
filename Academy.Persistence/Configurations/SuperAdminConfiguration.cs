using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class SuperAdminConfiguration : IEntityTypeConfiguration<SuperAdmin>
{
    public void Configure(EntityTypeBuilder<SuperAdmin> builder)
    {
        builder.ToTable("SuperAdmins");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
