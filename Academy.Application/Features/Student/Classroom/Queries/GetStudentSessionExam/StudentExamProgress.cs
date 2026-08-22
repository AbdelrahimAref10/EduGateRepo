using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom.Exams;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;

internal static class StudentExamProgress
{
    public static async Task ApplyExpiredQuestionsAsync(
        IApplicationDbContext dbContext,
        Exam exam,
        ExamAttempt attempt,
        CancellationToken cancellationToken)
    {
        if (attempt.SubmittedAtUtc.HasValue)
            return;

        var questions = exam.Questions
            .OrderBy(q => q.SortOrder)
            .ThenBy(q => q.Id)
            .ToList();

        if (questions.Count == 0)
            return;

        var answeredIds = attempt.Answers.Select(x => x.ExamQuestionId).ToHashSet();
        var changed = false;

        while (!attempt.SubmittedAtUtc.HasValue)
        {
            if (attempt.CurrentQuestionIndex < 0 || attempt.CurrentQuestionIndex >= questions.Count)
            {
                Complete(attempt, questions);
                changed = true;
                break;
            }

            var remaining = RemainingSeconds(exam, attempt);
            if (remaining > 0)
                break;

            var question = questions[attempt.CurrentQuestionIndex];
            if (!answeredIds.Contains(question.Id))
            {
                attempt.Answers.Add(new ExamAttemptAnswer
                {
                    ExamAttemptId = attempt.Id,
                    ExamQuestionId = question.Id,
                    SelectedOptionId = null,
                    IsCorrect = false
                });
                answeredIds.Add(question.Id);
            }

            attempt.CurrentQuestionIndex++;
            attempt.CurrentQuestionStartedAtUtc = DateTime.UtcNow;
            changed = true;

            if (attempt.CurrentQuestionIndex >= questions.Count)
                Complete(attempt, questions);
        }

        if (changed)
            await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static int RemainingSeconds(Exam exam, ExamAttempt attempt)
    {
        var allowed = exam.SecondsPerQuestion > 0 ? exam.SecondsPerQuestion : ExamRules.DefaultSecondsPerQuestion;
        var elapsed = (int)Math.Floor((DateTime.UtcNow - attempt.CurrentQuestionStartedAtUtc).TotalSeconds);
        return Math.Max(0, allowed - elapsed);
    }

    public static StudentExamDto ToDto(Exam exam, ExamAttempt? attempt)
    {
        var questions = exam.Questions
            .OrderBy(q => q.SortOrder)
            .ThenBy(q => q.Id)
            .ToList();

        var submitted = attempt?.SubmittedAtUtc is not null;
        var started = attempt is not null;
        var selected = attempt?.Answers.ToDictionary(x => x.ExamQuestionId, x => x.SelectedOptionId)
            ?? new Dictionary<int, int?>();

        StudentExamQuestionDto? current = null;
        int? currentNumber = null;
        int? remaining = null;

        if (started && !submitted && attempt is not null
            && attempt.CurrentQuestionIndex >= 0
            && attempt.CurrentQuestionIndex < questions.Count)
        {
            var question = questions[attempt.CurrentQuestionIndex];
            current = ToQuestionDto(question, selected, revealAnswers: false);
            currentNumber = attempt.CurrentQuestionIndex + 1;
            remaining = RemainingSeconds(exam, attempt);
        }

        return new StudentExamDto
        {
            Id = exam.Id,
            SessionId = exam.LessonGroupSessionId,
            Title = exam.Title,
            Status = (int)exam.Status,
            QuestionCount = questions.Count,
            SecondsPerQuestion = exam.SecondsPerQuestion > 0 ? exam.SecondsPerQuestion : ExamRules.DefaultSecondsPerQuestion,
            HasStarted = started,
            HasSubmitted = submitted,
            CurrentQuestionNumber = currentNumber,
            RemainingSeconds = remaining,
            Score = submitted ? attempt!.Score : null,
            MaxScore = submitted ? attempt!.MaxScore : questions.Count,
            Percentage = submitted && attempt!.MaxScore > 0
                ? Math.Round(attempt.Score * 100m / attempt.MaxScore, 1)
                : null,
            SubmittedAtUtc = attempt?.SubmittedAtUtc,
            CurrentQuestion = current,
            Questions = submitted
                ? questions.Select(q => ToQuestionDto(q, selected, revealAnswers: true)).ToList()
                : []
        };
    }

    public static void Complete(ExamAttempt attempt, IReadOnlyList<ExamQuestion> questions)
    {
        var score = 0;
        foreach (var question in questions)
        {
            var answer = attempt.Answers.FirstOrDefault(a => a.ExamQuestionId == question.Id);
            if (answer?.IsCorrect == true)
                score++;
        }

        attempt.Score = score;
        attempt.MaxScore = questions.Count;
        attempt.SubmittedAtUtc = DateTime.UtcNow;
        attempt.CurrentQuestionIndex = questions.Count;
    }

    private static StudentExamQuestionDto ToQuestionDto(
        ExamQuestion question,
        IReadOnlyDictionary<int, int?> selected,
        bool revealAnswers)
    {
        selected.TryGetValue(question.Id, out var selectedId);
        return new StudentExamQuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            SortOrder = question.SortOrder,
            SelectedOptionId = selectedId,
            Options = question.Options
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.Id)
                .Select(o => new StudentExamOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = revealAnswers ? o.IsCorrect : null,
                    SortOrder = o.SortOrder
                })
                .ToList()
        };
    }
}
