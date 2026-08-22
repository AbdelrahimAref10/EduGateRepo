using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Academy.Persistence.Configurations;

public sealed class ExamAttemptAnswerConfiguration : IEntityTypeConfiguration<ExamAttemptAnswer>
{
    public void Configure(EntityTypeBuilder<ExamAttemptAnswer> builder)
    {
        builder.ToTable("ExamAttemptAnswers");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Attempt)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.ExamAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ExamAttemptId, x.ExamQuestionId })
            .IsUnique();
    }
}
