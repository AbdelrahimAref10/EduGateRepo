using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Bio)
            .HasMaxLength(500);

        builder.Property(x => x.ProfilePhoto);

        builder.Property(x => x.PreferredLanguage)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(Academy.Domain.Enums.AppLanguage.Arabic);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(x => x.FullName);

        builder.HasOne(x => x.Area)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StudentProfile)
            .WithOne(x => x.User)
            .HasForeignKey<Student>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TeacherProfile)
            .WithOne(x => x.User)
            .HasForeignKey<Teacher>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SuperAdminProfile)
            .WithOne(x => x.User)
            .HasForeignKey<SuperAdmin>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
