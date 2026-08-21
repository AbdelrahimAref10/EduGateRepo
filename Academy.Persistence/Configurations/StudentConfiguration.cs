using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.IsParent)
            .IsRequired();

        builder.Property(x => x.StudentCode)
            .HasMaxLength(32);

        builder.HasIndex(x => x.StudentCode)
            .IsUnique()
            .HasFilter("[StudentCode] IS NOT NULL");

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
