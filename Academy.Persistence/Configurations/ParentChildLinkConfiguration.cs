using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class ParentChildLinkConfiguration : IEntityTypeConfiguration<ParentChildLink>
{
    public void Configure(EntityTypeBuilder<ParentChildLink> builder)
    {
        builder.ToTable("ParentChildLinks");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.ParentStudent)
            .WithMany()
            .HasForeignKey(x => x.ParentStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ChildStudent)
            .WithMany()
            .HasForeignKey(x => x.ChildStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ParentStudentId, x.ChildStudentId })
            .IsUnique();

        builder.HasIndex(x => x.ChildStudentId);

        builder.Property(x => x.LinkedAtUtc)
            .IsRequired();
    }
}
