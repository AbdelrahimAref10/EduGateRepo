using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class LessonGroupMemberConfiguration : IEntityTypeConfiguration<LessonGroupMember>
{
    public void Configure(EntityTypeBuilder<LessonGroupMember> builder)
    {
        builder.ToTable("LessonGroupMembers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AddedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.LessonGroup)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.LessonGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.GroupMemberships)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LessonGroupId, x.StudentId })
            .IsUnique();

        builder.HasIndex(x => x.StudentId);
    }
}
