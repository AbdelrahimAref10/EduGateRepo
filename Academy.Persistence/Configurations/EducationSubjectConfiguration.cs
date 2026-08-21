using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class EducationSubjectConfiguration : IEntityTypeConfiguration<EducationSubject>
{
    public void Configure(EntityTypeBuilder<EducationSubject> builder)
    {
        builder.ToTable("EducationSubjects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.EducationYear)
            .WithMany(x => x.Subjects)
            .HasForeignKey(x => x.EducationYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EducationYearId, x.SortOrder });
        builder.HasIndex(x => new { x.EducationYearId, x.NameEn });
    }
}
