using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RatingAverage)
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(x => x.RatingCount)
            .IsRequired();
    }
}
