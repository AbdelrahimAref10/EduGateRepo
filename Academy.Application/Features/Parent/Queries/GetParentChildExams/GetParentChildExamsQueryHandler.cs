using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Application.Features.Parent.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetParentChildExams;

public sealed record GetParentChildExamsQuery(
    int UserId,
    int? ChildStudentId = null,
    int? Page = null,
    int? PageSize = null) : IRequest<Result<PagedResult<ParentExamListItemDto>>>;

public sealed class GetParentChildExamsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetParentChildExamsQuery, Result<PagedResult<ParentExamListItemDto>>>
{
    public async Task<Result<PagedResult<ParentExamListItemDto>>> Handle(
        GetParentChildExamsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = Paging.Normalize(request.Page, request.PageSize);

        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<PagedResult<ParentExamListItemDto>>.NotFound("Parent profile was not found.");

        var linkedIds = await ParentAccess.GetLinkedChildStudentIdsAsync(
            dbContext, parentStudentId.Value, cancellationToken);

        if (linkedIds.Count == 0)
            return Result<PagedResult<ParentExamListItemDto>>.Success(
                PagedResult<ParentExamListItemDto>.Empty(page, pageSize));

        IReadOnlyList<int> childIds = linkedIds;
        if (request.ChildStudentId is > 0)
        {
            if (!linkedIds.Contains(request.ChildStudentId.Value))
                return Result<PagedResult<ParentExamListItemDto>>.Failure(
                    "This child is not linked to your account.", 403);

            childIds = [request.ChildStudentId.Value];
        }

        var memberships = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(m => childIds.Contains(m.StudentId))
            .Select(m => new { m.StudentId, m.LessonGroupId })
            .ToListAsync(cancellationToken);

        if (memberships.Count == 0)
            return Result<PagedResult<ParentExamListItemDto>>.Success(
                PagedResult<ParentExamListItemDto>.Empty(page, pageSize));

        var groupIds = memberships.Select(m => m.LessonGroupId).Distinct().ToList();
        var childIdsByGroup = memberships
            .GroupBy(m => m.LessonGroupId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StudentId).Distinct().ToList());

        var childNames = await dbContext.Students
            .AsNoTracking()
            .Where(s => childIds.Contains(s.Id))
            .Select(s => new { s.Id, Name = s.User.FullName })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var exams = await dbContext.Exams
            .AsNoTracking()
            .Where(exam =>
                exam.Status == ExamStatus.Published
                && groupIds.Contains(exam.LessonGroupSession.LessonGroupId))
            .OrderByDescending(exam => exam.LessonGroupSession.SessionDate)
            .ThenByDescending(exam => exam.LessonGroupSession.StartTime)
            .Select(exam => new
            {
                ExamId = exam.Id,
                SessionId = exam.LessonGroupSessionId,
                GroupId = exam.LessonGroupSession.LessonGroupId,
                exam.Title,
                Subject = exam.LessonGroupSession.LessonGroup.Lesson.Subject,
                GroupName = exam.LessonGroupSession.LessonGroup.Name,
                TeacherFirst = exam.LessonGroupSession.LessonGroup.Lesson.Teacher.User.FirstName,
                TeacherLast = exam.LessonGroupSession.LessonGroup.Lesson.Teacher.User.LastName,
                SessionDate = exam.LessonGroupSession.SessionDate,
                StartTime = exam.LessonGroupSession.StartTime,
                QuestionCount = exam.Questions.Count,
                Attempts = exam.Attempts
                    .Where(a => childIds.Contains(a.StudentId))
                    .Select(a => new
                    {
                        a.StudentId,
                        Submitted = a.SubmittedAtUtc != null,
                        a.Score,
                        a.MaxScore
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var flat = new List<ParentExamListItemDto>();
        foreach (var exam in exams)
        {
            if (!childIdsByGroup.TryGetValue(exam.GroupId, out var kids))
                continue;

            var teacherName = $"{exam.TeacherFirst} {exam.TeacherLast}".Trim();
            foreach (var childId in kids)
            {
                var attempt = exam.Attempts.FirstOrDefault(a => a.StudentId == childId);
                var submitted = attempt?.Submitted == true;
                var maxScore = attempt?.MaxScore ?? 0;

                flat.Add(new ParentExamListItemDto
                {
                    ExamId = exam.ExamId,
                    SessionId = exam.SessionId,
                    ChildStudentId = childId,
                    ChildName = childNames.GetValueOrDefault(childId, ""),
                    Title = exam.Title,
                    Subject = exam.Subject,
                    GroupName = exam.GroupName,
                    TeacherName = teacherName,
                    SessionDate = exam.SessionDate,
                    StartTime = exam.StartTime,
                    QuestionCount = exam.QuestionCount,
                    HasSubmitted = submitted,
                    Score = submitted ? attempt!.Score : null,
                    MaxScore = submitted ? attempt!.MaxScore : null,
                    Percentage = submitted && maxScore > 0
                        ? Math.Round(attempt!.Score * 100m / maxScore, 1)
                        : null
                });
            }
        }

        var totalCount = flat.Count;
        var items = flat.Skip(skip).Take(pageSize).ToList();

        return Result<PagedResult<ParentExamListItemDto>>.Success(
            PagedResult<ParentExamListItemDto>.Create(items, totalCount, page, pageSize));
    }
}
