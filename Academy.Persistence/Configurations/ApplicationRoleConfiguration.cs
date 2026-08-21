using Academy.Domain.Common;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("Roles");

        builder.HasData(
            CreateRole(AppRoles.Ids.SuperAdmin, AppRoles.SuperAdmin, "super-admin-stamp"),
            CreateRole(AppRoles.Ids.Teacher, AppRoles.Teacher, "teacher-stamp"),
            CreateRole(AppRoles.Ids.Student, AppRoles.Student, "student-stamp"),
            CreateRole(AppRoles.Ids.Parent, AppRoles.Parent, "parent-stamp"));
    }

    private static ApplicationRole CreateRole(int id, string name, string concurrencyStamp) =>
        new()
        {
            Id = id,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            ConcurrencyStamp = concurrencyStamp
        };
}
