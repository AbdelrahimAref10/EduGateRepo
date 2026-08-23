using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetMyStudentExams;

public sealed record GetMyStudentExamsQuery(int UserId, int? Page = null, int? PageSize = null)
    : IRequest<Result<PagedResult<StudentExamListItemDto>>>;

public sealed class GetMyStudentExamsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyStudentExamsQuery, Result<PagedResult<StudentExamListItemDto>>>
{
    public async Task<Result<PagedResult<StudentExamListItemDto>>> Handle(
        GetMyStudentExamsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = Paging.Normalize(request.Page, request.PageSize);

        var studentId = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && !x.IsParent)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (studentId is null)
            return Result<PagedResult<StudentExamListItemDto>>.NotFound("Student profile was not found.");

        var groupIds = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(m => m.StudentId == studentId.Value)
            .Select(m => m.LessonGroupId)
            .ToListAsync(cancellationToken);

        if (groupIds.Count == 0)
            return Result<PagedResult<StudentExamListItemDto>>.Success(PagedResult<StudentExamListItemDto>.Empty(page, pageSize));

        var baseQuery = dbContext.Exams
            .AsNoTracking()
            .Where(exam =>
                exam.Status == ExamStatus.Published
                && groupIds.Contains(exam.LessonGroupSession.LessonGroupId));

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<StudentExamListItemDto>>.Success(PagedResult<StudentExamListItemDto>.Empty(page, pageSize));

        var rows = await baseQuery
            .OrderByDescending(exam => exam.LessonGroupSession.SessionDate)
            .ThenByDescending(exam => exam.LessonGroupSession.StartTime)
            .ThenByDescending(exam => exam.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(exam => new
            {
                ExamId = exam.Id,
                SessionId = exam.LessonGroupSessionId,
                LessonId = exam.LessonGroupSession.LessonGroup.LessonId,
                GroupId = exam.LessonGroupSession.LessonGroupId,
                exam.Title,
                Subject = exam.LessonGroupSession.LessonGroup.Lesson.Subject,
                GroupName = exam.LessonGroupSession.LessonGroup.Name,
                TeacherFirstName = exam.LessonGroupSession.LessonGroup.Lesson.Teacher.User.FirstName,
                TeacherLastName = exam.LessonGroupSession.LessonGroup.Lesson.Teacher.User.LastName,
                SessionDate = exam.LessonGroupSession.SessionDate,
                StartTime = exam.LessonGroupSession.StartTime,
                QuestionCount = exam.Questions.Count,
                SessionStarted = exam.LessonGroupSession.StartedAtUtc != null,
                Attempt = exam.Attempts
                    .Where(a => a.StudentId == studentId.Value)
                    .Select(a => new
                    {
                        Submitted = a.SubmittedAtUtc != null,
                        a.Score,
                        a.MaxScore
                    })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var sessionNumbers = await BuildSessionNumbersAsync(
            rows.Select(r => r.GroupId).Distinct().ToList(),
            cancellationToken);

        var items = rows.Select(r =>
        {
            var attempt = r.Attempt;
            var submitted = attempt?.Submitted == true;
            var maxScore = attempt?.MaxScore ?? 0;
            var teacherName = $"{r.TeacherFirstName} {r.TeacherLastName}".Trim();

            return new StudentExamListItemDto
            {
                ExamId = r.ExamId,
                SessionId = r.SessionId,
                LessonId = r.LessonId,
                Title = r.Title,
                Subject = r.Subject,
                GroupName = r.GroupName,
                SessionNumber = sessionNumbers.GetValueOrDefault(r.SessionId, 0),
                Topic = null,
                TeacherName = teacherName,
                SessionDate = r.SessionDate,
                StartTime = r.StartTime,
                QuestionCount = r.QuestionCount,
                SessionStarted = r.SessionStarted,
                HasStarted = attempt is not null,
                HasSubmitted = submitted,
                Score = submitted ? attempt!.Score : null,
                MaxScore = submitted ? attempt!.MaxScore : null,
                Percentage = submitted && maxScore > 0
                    ? Math.Round(attempt!.Score * 100m / maxScore, 1)
                    : null,
                CanTake = r.SessionStarted && !submitted
            };
        }).ToList();

        return Result<PagedResult<StudentExamListItemDto>>.Success(
            PagedResult<StudentExamListItemDto>.Create(items, totalCount, page, pageSize));
    }

    private async Task<Dictionary<int, int>> BuildSessionNumbersAsync(
        IReadOnlyList<int> groupIds,
        CancellationToken cancellationToken)
    {
        if (groupIds.Count == 0)
            return new Dictionary<int, int>();

        var sessions = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(s => groupIds.Contains(s.LessonGroupId))
            .Select(s => new
            {
                s.Id,
                s.LessonGroupId,
                s.SessionDate,
                s.StartTime
            })
            .ToListAsync(cancellationToken);

        var ranks = new Dictionary<int, int>(sessions.Count);
        foreach (var group in sessions.GroupBy(s => s.LessonGroupId))
        {
            var ordered = group
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .ThenBy(s => s.Id);

            var number = 0;
            foreach (var session in ordered)
                ranks[session.Id] = ++number;
        }

        return ranks;
    }
}
