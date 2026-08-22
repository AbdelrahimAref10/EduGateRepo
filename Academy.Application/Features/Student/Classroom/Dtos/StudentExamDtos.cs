namespace Academy.Application.Features.Student.Classroom.Dtos;

public sealed class StudentExamOptionDto
{
    public required int Id { get; init; }

    public required string Text { get; init; }

    public bool? IsCorrect { get; init; }

    public required int SortOrder { get; init; }
}

public sealed class StudentExamQuestionDto
{
    public required int Id { get; init; }

    public required string Text { get; init; }

    public required int SortOrder { get; init; }

    public int? SelectedOptionId { get; init; }

    public IReadOnlyList<StudentExamOptionDto> Options { get; init; } = [];
}

public sealed class StudentExamDto
{
    public required int Id { get; init; }

    public required int SessionId { get; init; }

    public required string Title { get; init; }

    public required int Status { get; init; }

    public required int QuestionCount { get; init; }

    public required int SecondsPerQuestion { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasSubmitted { get; init; }

    public int? CurrentQuestionNumber { get; init; }

    public int? RemainingSeconds { get; init; }

    public int? Score { get; init; }

    public int? MaxScore { get; init; }

    public decimal? Percentage { get; init; }

    public DateTime? SubmittedAtUtc { get; init; }

    public StudentExamQuestionDto? CurrentQuestion { get; init; }

    public IReadOnlyList<StudentExamQuestionDto> Questions { get; init; } = [];
}

public sealed class AnswerStudentExamQuestionRequest
{
    public int? OptionId { get; set; }
}

public sealed class StudentExamListItemDto
{
    public required int ExamId { get; init; }

    public required int SessionId { get; init; }

    public required int LessonId { get; init; }

    public required string Title { get; init; }

    public required string Subject { get; init; }

    public required string GroupName { get; init; }

    public string? Topic { get; init; }

    public required string TeacherName { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public required int QuestionCount { get; init; }

    public required bool SessionStarted { get; init; }

    public required bool HasStarted { get; init; }

    public required bool HasSubmitted { get; init; }

    public int? Score { get; init; }

    public int? MaxScore { get; init; }

    public decimal? Percentage { get; init; }

    public required bool CanTake { get; init; }
}
