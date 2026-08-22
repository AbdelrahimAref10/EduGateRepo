using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Entities;

namespace Academy.Application.Features.Teacher.Classroom;

internal static class TeacherExamReviewMapper
{
    public static IReadOnlyList<TeacherExamReviewQuestionDto> ToQuestions(Exam exam, ExamAttempt? attempt)
    {
        var selected = attempt?.Answers.ToDictionary(x => x.ExamQuestionId, x => x.SelectedOptionId)
            ?? new Dictionary<int, int?>();

        return exam.Questions
            .OrderBy(q => q.SortOrder)
            .ThenBy(q => q.Id)
            .Select(question =>
            {
                selected.TryGetValue(question.Id, out var selectedId);
                return new TeacherExamReviewQuestionDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    SortOrder = question.SortOrder,
                    SelectedOptionId = selectedId,
                    Options = question.Options
                        .OrderBy(o => o.SortOrder)
                        .ThenBy(o => o.Id)
                        .Select(o => new TeacherExamReviewOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            IsCorrect = o.IsCorrect,
                            SortOrder = o.SortOrder
                        })
                        .ToList()
                };
            })
            .ToList();
    }
}
