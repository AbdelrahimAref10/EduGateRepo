namespace Academy.Application.Features.Teacher.Classroom.Dtos;

public sealed class GenerateSessionExamRequest
{
    public int QuestionCount { get; set; } = 10;
}

public sealed class TeacherExamOptionDto
{
    public required int Id { get; init; }

    public required string Text { get; init; }

    public required bool IsCorrect { get; init; }

    public required int SortOrder { get; init; }
}

public sealed class TeacherExamQuestionDto
{
    public required int Id { get; init; }

    public required string Text { get; init; }

    public required int SortOrder { get; init; }

    public IReadOnlyList<TeacherExamOptionDto> Options { get; init; } = [];
}

public sealed class TeacherExamDto
{
    public required int Id { get; init; }

    public required int SessionId { get; init; }

    public required string Title { get; init; }

    public required int Status { get; init; }

    public required string StatusName { get; init; }

    public required int QuestionCount { get; init; }

    public required int SecondsPerQuestion { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? PublishedAtUtc { get; init; }

    public IReadOnlyList<TeacherExamQuestionDto> Questions { get; init; } = [];
}

public sealed class TeacherExamResultRowDto
{
    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required bool HasSubmitted { get; init; }

    public int? Score { get; init; }

    public int? MaxScore { get; init; }

    public decimal? Percentage { get; init; }

    public DateTime? SubmittedAtUtc { get; init; }

    public IReadOnlyList<TeacherExamReviewQuestionDto> Questions { get; init; } = [];
}

public sealed class TeacherExamResultsDto
{
    public required int ExamId { get; init; }

    public required string Title { get; init; }

    public required int Status { get; init; }

    public required int SubmittedCount { get; init; }

    public required int StudentCount { get; init; }

    public IReadOnlyList<TeacherExamResultRowDto> Students { get; init; } = [];
}

public sealed class TeacherExamReviewOptionDto
{
    public required int Id { get; init; }

    public required string Text { get; init; }

    public required bool IsCorrect { get; init; }

    public required int SortOrder { get; init; }
}

public sealed class TeacherExamReviewQuestionDto
{
    public required int Id { get; init; }

    public required string Text { get; init; }

    public required int SortOrder { get; init; }

    public int? SelectedOptionId { get; init; }

    public IReadOnlyList<TeacherExamReviewOptionDto> Options { get; init; } = [];
}

public sealed class TeacherStudentExamReviewDto
{
    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required string Title { get; init; }

    public required bool HasSubmitted { get; init; }

    public int? Score { get; init; }

    public int? MaxScore { get; init; }

    public decimal? Percentage { get; init; }

    public DateTime? SubmittedAtUtc { get; init; }

    public IReadOnlyList<TeacherExamReviewQuestionDto> Questions { get; init; } = [];
}
