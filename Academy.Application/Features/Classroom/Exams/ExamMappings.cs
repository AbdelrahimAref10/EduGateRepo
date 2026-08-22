using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Entities;

namespace Academy.Application.Features.Classroom.Exams;

internal static class ExamMappings
{
    public static TeacherExamDto ToTeacherDto(Exam exam) =>
        new()
        {
            Id = exam.Id,
            SessionId = exam.LessonGroupSessionId,
            Title = exam.Title,
            Status = (int)exam.Status,
            StatusName = exam.Status.ToString(),
            QuestionCount = exam.Questions.Count,
            SecondsPerQuestion = exam.SecondsPerQuestion,
            CreatedAtUtc = exam.CreatedAtUtc,
            PublishedAtUtc = exam.PublishedAtUtc,
            Questions = exam.Questions
                .OrderBy(q => q.SortOrder)
                .ThenBy(q => q.Id)
                .Select(q => new TeacherExamQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    SortOrder = q.SortOrder,
                    Options = q.Options
                        .OrderBy(o => o.SortOrder)
                        .ThenBy(o => o.Id)
                        .Select(o => new TeacherExamOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            IsCorrect = o.IsCorrect,
                            SortOrder = o.SortOrder
                        })
                        .ToList()
                })
                .ToList()
        };

    public static Exam ToExamEntity(
        int sessionId,
        int createdByUserId,
        Academy.Application.Contracts.Ai.GeneratedExam generated,
        int secondsPerQuestion)
    {
        var exam = new Exam
        {
            LessonGroupSessionId = sessionId,
            Title = generated.Title,
            Status = Domain.Enums.ExamStatus.Published,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow,
            PublishedAtUtc = DateTime.UtcNow,
            SecondsPerQuestion = secondsPerQuestion > 0
                ? secondsPerQuestion
                : ExamRules.DefaultSecondsPerQuestion
        };

        var questionOrder = 0;
        foreach (var question in generated.Questions)
        {
            var entity = new ExamQuestion
            {
                Text = question.Text,
                SortOrder = questionOrder++
            };

            var optionOrder = 0;
            foreach (var option in question.Options)
            {
                entity.Options.Add(new ExamQuestionOption
                {
                    Text = option.Text,
                    IsCorrect = option.IsCorrect,
                    SortOrder = optionOrder++
                });
            }

            exam.Questions.Add(entity);
        }

        return exam;
    }
}
