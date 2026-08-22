using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetMyStudentExams;

public sealed class GetMyStudentExamsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyStudentExamsQuery, Result<IReadOnlyList<StudentExamListItemDto>>>
{
    public async Task<Result<IReadOnlyList<StudentExamListItemDto>>> Handle(
        GetMyStudentExamsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<IReadOnlyList<StudentExamListItemDto>>.NotFound("Student profile was not found.");

        var items = await dbContext.Exams
            .AsNoTracking()
            .Where(x => x.Status == ExamStatus.Published
                        && x.LessonGroupSession.LessonGroup.Members.Any(m => m.StudentId == student.Id))
            .OrderByDescending(x => x.LessonGroupSession.SessionDate)
            .ThenByDescending(x => x.LessonGroupSession.StartTime)
            .Select(x => new StudentExamListItemDto
            {
                ExamId = x.Id,
                SessionId = x.LessonGroupSessionId,
                LessonId = x.LessonGroupSession.LessonGroup.LessonId,
                Title = x.Title,
                Subject = x.LessonGroupSession.LessonGroup.Lesson.Subject,
                GroupName = x.LessonGroupSession.LessonGroup.Name,
                Topic = x.LessonGroupSession.Topic,
                TeacherName = (x.LessonGroupSession.LessonGroup.Lesson.Teacher.User.FirstName + " "
                               + x.LessonGroupSession.LessonGroup.Lesson.Teacher.User.LastName).Trim(),
                SessionDate = x.LessonGroupSession.SessionDate,
                StartTime = x.LessonGroupSession.StartTime,
                QuestionCount = x.Questions.Count,
                SessionStarted = x.LessonGroupSession.StartedAtUtc != null,
                HasStarted = x.Attempts.Any(a => a.StudentId == student.Id),
                HasSubmitted = x.Attempts.Any(a => a.StudentId == student.Id && a.SubmittedAtUtc != null),
                Score = x.Attempts
                    .Where(a => a.StudentId == student.Id && a.SubmittedAtUtc != null)
                    .Select(a => (int?)a.Score)
                    .FirstOrDefault(),
                MaxScore = x.Attempts
                    .Where(a => a.StudentId == student.Id && a.SubmittedAtUtc != null)
                    .Select(a => (int?)a.MaxScore)
                    .FirstOrDefault(),
                Percentage = x.Attempts
                    .Where(a => a.StudentId == student.Id && a.SubmittedAtUtc != null && a.MaxScore > 0)
                    .Select(a => (decimal?)Math.Round(a.Score * 100m / a.MaxScore, 1))
                    .FirstOrDefault(),
                CanTake = x.LessonGroupSession.StartedAtUtc != null
                          && !x.Attempts.Any(a => a.StudentId == student.Id && a.SubmittedAtUtc != null)
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StudentExamListItemDto>>.Success(items);
    }
}
