using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        builder.ToTable("RoleClaims");

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RoleClaims)
            .HasForeignKey(x => x.RoleId)
            .IsRequired();
    }
}
