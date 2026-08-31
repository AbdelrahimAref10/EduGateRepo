using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.LearningPath.Dtos;
using Academy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.LearningPath.Common;

internal static class LearningPathQueries
{
    public static (DateOnly WeekStart, DateOnly WeekEnd) CurrentWeekUtc()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var mondayOffset = ((int)today.DayOfWeek + 6) % 7;
        var start = today.AddDays(-mondayOffset);
        return (start, start.AddDays(6));
    }

    public static async Task<WeeklyLearningPlanDto> BuildWeeklyPlanAsync(
        IApplicationDbContext db,
        IReadOnlyList<int> studentIds,
        IReadOnlyDictionary<int, string> namesByStudent,
        int? teacherId,
        CancellationToken cancellationToken)
    {
        var (weekStart, weekEnd) = CurrentWeekUtc();
        if (studentIds.Count == 0)
        {
            return new WeeklyLearningPlanDto
            {
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Sessions = [],
                UnpaidCharges = [],
                ExamsDue = []
            };
        }

        var memberships = await LoadMembershipsAsync(db, studentIds, teacherId, cancellationToken);
        var groupIds = memberships.Select(m => m.LessonGroupId).Distinct().ToList();
        var childIdsByGroup = memberships
            .GroupBy(m => m.LessonGroupId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StudentId).Distinct().ToList());

        IReadOnlyList<WeeklyPlanSessionDto> sessions = [];
        if (groupIds.Count > 0)
        {
            var rows = await db.LessonGroupSessions
                .AsNoTracking()
                .Where(s =>
                    groupIds.Contains(s.LessonGroupId)
                    && s.SessionDate >= weekStart
                    && s.SessionDate <= weekEnd)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    s.Id,
                    s.LessonGroupId,
                    LessonId = s.LessonGroup.LessonId,
                    Subject = s.LessonGroup.Lesson.Subject,
                    GroupName = s.LessonGroup.Name,
                    TeacherFirst = s.LessonGroup.Lesson.Teacher.User.FirstName,
                    TeacherLast = s.LessonGroup.Lesson.Teacher.User.LastName,
                    s.SessionDate,
                    s.StartTime,
                    s.Topic,
                    HasStarted = s.StartedAtUtc != null
                })
                .ToListAsync(cancellationToken);

            var sessionIds = rows.Select(r => r.Id).ToList();
            var details = await db.LessonSessionStudentDetails
                .AsNoTracking()
                .Where(d => sessionIds.Contains(d.LessonGroupSessionId) && studentIds.Contains(d.StudentId))
                .Select(d => new { d.LessonGroupSessionId, d.StudentId, d.IsPresent, d.TeacherNotes })
                .ToListAsync(cancellationToken);

            var detailMap = details.ToDictionary(d => (d.LessonGroupSessionId, d.StudentId));

            sessions = rows
                .SelectMany(s =>
                {
                    if (!childIdsByGroup.TryGetValue(s.LessonGroupId, out var kids))
                        return [];

                    var teacherName = $"{s.TeacherFirst} {s.TeacherLast}".Trim();
                    return kids.Select(childId =>
                    {
                        detailMap.TryGetValue((s.Id, childId), out var detail);
                        return new WeeklyPlanSessionDto
                        {
                            SessionId = s.Id,
                            LessonId = s.LessonId,
                            StudentId = childId,
                            StudentName = namesByStudent.GetValueOrDefault(childId, ""),
                            Subject = s.Subject,
                            GroupName = s.GroupName,
                            TeacherName = teacherName,
                            SessionDate = s.SessionDate,
                            StartTime = s.StartTime,
                            Topic = s.Topic,
                            Notes = detail?.TeacherNotes,
                            HasStarted = s.HasStarted,
                            IsPresent = detail?.IsPresent
                        };
                    });
                })
                .OrderBy(x => x.SessionDate)
                .ThenBy(x => x.StartTime)
                .ToList();
        }

        var unpaid = await db.Charges
            .AsNoTracking()
            .Where(c =>
                studentIds.Contains(c.StudentId)
                && (teacherId == null || c.TeacherId == teacherId)
                && c.Status != ChargeStatus.Deferred
                && c.Status != ChargeStatus.Paid
                && c.Amount > c.AllocatedAmount)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(40)
            .Select(c => new WeeklyPlanChargeDto
            {
                ChargeId = c.Id,
                StudentId = c.StudentId,
                StudentName = c.Student.User.FullName,
                LessonId = c.LessonId,
                Subject = c.Lesson.Subject,
                Type = c.Type.ToString(),
                Remaining = c.Amount - c.AllocatedAmount
            })
            .ToListAsync(cancellationToken);

        IReadOnlyList<WeeklyPlanExamDto> examsDue = [];
        if (groupIds.Count > 0)
        {
            var examRows = await db.Exams
                .AsNoTracking()
                .Where(exam =>
                    exam.Status == ExamStatus.Published
                    && groupIds.Contains(exam.LessonGroupSession.LessonGroupId)
                    && exam.LessonGroupSession.SessionDate >= weekStart
                    && exam.LessonGroupSession.SessionDate <= weekEnd)
                .Select(exam => new
                {
                    ExamId = exam.Id,
                    SessionId = exam.LessonGroupSessionId,
                    GroupId = exam.LessonGroupSession.LessonGroupId,
                    exam.Title,
                    Subject = exam.LessonGroupSession.LessonGroup.Lesson.Subject,
                    SessionDate = exam.LessonGroupSession.SessionDate,
                    SessionStarted = exam.LessonGroupSession.StartedAtUtc != null,
                    Attempts = exam.Attempts
                        .Where(a => studentIds.Contains(a.StudentId))
                        .Select(a => new { a.StudentId, Submitted = a.SubmittedAtUtc != null })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            examsDue = examRows
                .SelectMany(exam =>
                {
                    if (!childIdsByGroup.TryGetValue(exam.GroupId, out var kids))
                        return [];

                    return kids.Select(childId =>
                    {
                        var submitted = exam.Attempts.Any(a => a.StudentId == childId && a.Submitted);
                        return new WeeklyPlanExamDto
                        {
                            ExamId = exam.ExamId,
                            SessionId = exam.SessionId,
                            StudentId = childId,
                            StudentName = namesByStudent.GetValueOrDefault(childId, ""),
                            Title = exam.Title,
                            Subject = exam.Subject,
                            SessionDate = exam.SessionDate,
                            SessionStarted = exam.SessionStarted,
                            HasSubmitted = submitted
                        };
                    }).Where(x => !x.HasSubmitted);
                })
                .OrderBy(x => x.SessionDate)
                .ToList();
        }

        return new WeeklyLearningPlanDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            Sessions = sessions,
            UnpaidCharges = unpaid,
            ExamsDue = examsDue
        };
    }

    public static async Task<IReadOnlyList<LessonProgressDto>> BuildProgressAsync(
        IApplicationDbContext db,
        IReadOnlyList<int> studentIds,
        IReadOnlyDictionary<int, string> namesByStudent,
        int? teacherId,
        int? lessonId,
        CancellationToken cancellationToken,
        int? lessonGroupId = null)
    {
        if (studentIds.Count == 0)
            return [];

        var memberships = await LoadMembershipsAsync(db, studentIds, teacherId, cancellationToken);
        if (lessonId is > 0)
            memberships = memberships.Where(m => m.LessonId == lessonId.Value).ToList();
        if (lessonGroupId is > 0)
            memberships = memberships.Where(m => m.LessonGroupId == lessonGroupId.Value).ToList();

        if (memberships.Count == 0)
            return [];

        var groupIds = memberships.Select(m => m.LessonGroupId).Distinct().ToList();
        var lessonIds = memberships.Select(m => m.LessonId).Distinct().ToList();

        var sessions = await db.LessonGroupSessions
            .AsNoTracking()
            .Where(s => groupIds.Contains(s.LessonGroupId) && !s.IsMakeup)
            .Select(s => new
            {
                s.Id,
                s.LessonGroupId,
                s.SessionDate,
                s.StartTime,
                s.Topic,
                HasStarted = s.StartedAtUtc != null
            })
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var details = sessionIds.Count == 0
            ? []
            : await db.LessonSessionStudentDetails
                .AsNoTracking()
                .Where(d => sessionIds.Contains(d.LessonGroupSessionId) && studentIds.Contains(d.StudentId))
                .Select(d => new { d.LessonGroupSessionId, d.StudentId, d.IsPresent, d.TeacherNotes })
                .ToListAsync(cancellationToken);

        var detailMap = details.ToDictionary(d => (d.LessonGroupSessionId, d.StudentId));

        var examRows = await db.Exams
            .AsNoTracking()
            .Where(exam =>
                exam.Status == ExamStatus.Published
                && groupIds.Contains(exam.LessonGroupSession.LessonGroupId))
            .Select(exam => new
            {
                GroupId = exam.LessonGroupSession.LessonGroupId,
                Attempts = exam.Attempts
                    .Where(a => studentIds.Contains(a.StudentId) && a.SubmittedAtUtc != null)
                    .Select(a => new { a.StudentId, a.Score, a.MaxScore })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var outstanding = await db.Charges
            .AsNoTracking()
            .Where(c =>
                studentIds.Contains(c.StudentId)
                && lessonIds.Contains(c.LessonId)
                && c.Status != ChargeStatus.Deferred
                && c.Status != ChargeStatus.Paid
                && c.Amount > c.AllocatedAmount)
            .GroupBy(c => new { c.StudentId, c.LessonId })
            .Select(g => new
            {
                g.Key.StudentId,
                g.Key.LessonId,
                Remaining = g.Sum(c => c.Amount - c.AllocatedAmount)
            })
            .ToListAsync(cancellationToken);

        var outstandingMap = outstanding.ToDictionary(x => (x.StudentId, x.LessonId), x => x.Remaining);
        var sessionsByGroup = sessions.GroupBy(s => s.LessonGroupId).ToDictionary(g => g.Key, g => g.ToList());
        var examsByGroup = examRows.GroupBy(e => e.GroupId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<LessonProgressDto>(memberships.Count);
        foreach (var m in memberships)
        {
            var groupSessions = sessionsByGroup.GetValueOrDefault(m.LessonGroupId) ?? [];
            var held = groupSessions.Where(s => s.HasStarted).ToList();
            var present = held.Count(s =>
                detailMap.TryGetValue((s.Id, m.StudentId), out var d) && d.IsPresent);

            var attempts = (examsByGroup.GetValueOrDefault(m.LessonGroupId) ?? [])
                .SelectMany(e => e.Attempts.Where(a => a.StudentId == m.StudentId))
                .ToList();
            decimal? examAvg = null;
            if (attempts.Count > 0)
            {
                var percents = attempts
                    .Where(a => a.MaxScore > 0)
                    .Select(a => a.Score * 100m / a.MaxScore)
                    .ToList();
                if (percents.Count > 0)
                    examAvg = Math.Round(percents.Average(), 1);
            }

            var recent = groupSessions
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.StartTime)
                .Take(5)
                .Select(s =>
                {
                    detailMap.TryGetValue((s.Id, m.StudentId), out var d);
                    return new RecentSessionProgressDto
                    {
                        SessionId = s.Id,
                        SessionDate = s.SessionDate,
                        StartTime = s.StartTime,
                        Topic = s.Topic,
                        TeacherNotes = d?.TeacherNotes,
                        HasStarted = s.HasStarted,
                        IsPresent = d?.IsPresent
                    };
                })
                .ToList();

            result.Add(new LessonProgressDto
            {
                StudentId = m.StudentId,
                StudentName = namesByStudent.GetValueOrDefault(m.StudentId, m.StudentName),
                LessonId = m.LessonId,
                GroupId = m.LessonGroupId,
                Subject = m.Subject,
                GroupName = m.GroupName,
                TeacherName = m.TeacherName,
                SessionsHeld = held.Count,
                SessionsPresent = present,
                AttendancePercent = held.Count > 0
                    ? Math.Round(present * 100m / held.Count, 1)
                    : null,
                ExamsTaken = attempts.Count,
                ExamAveragePercent = examAvg,
                Outstanding = outstandingMap.GetValueOrDefault((m.StudentId, m.LessonId)),
                RecentSessions = recent
            });
        }

        return result
            .OrderBy(x => x.StudentName)
            .ThenBy(x => x.Subject)
            .ToList();
    }

    private static async Task<List<MembershipRow>> LoadMembershipsAsync(
        IApplicationDbContext db,
        IReadOnlyList<int> studentIds,
        int? teacherId,
        CancellationToken cancellationToken)
    {
        var query = db.LessonGroupMembers
            .AsNoTracking()
            .Where(m => studentIds.Contains(m.StudentId) && m.LessonGroup.EndedAtUtc == null);

        if (teacherId is > 0)
            query = query.Where(m => m.LessonGroup.Lesson.TeacherId == teacherId.Value);

        var rows = await query
            .Select(m => new
            {
                m.StudentId,
                StudentName = m.Student.User.FullName,
                m.LessonGroupId,
                m.LessonGroup.LessonId,
                m.LessonGroup.Lesson.Subject,
                GroupName = m.LessonGroup.Name,
                TeacherFirst = m.LessonGroup.Lesson.Teacher.User.FirstName,
                TeacherLast = m.LessonGroup.Lesson.Teacher.User.LastName
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(m => new MembershipRow(
                m.StudentId,
                m.StudentName,
                m.LessonGroupId,
                m.LessonId,
                m.Subject,
                m.GroupName,
                $"{m.TeacherFirst} {m.TeacherLast}".Trim()))
            .ToList();
    }

    private sealed record MembershipRow(
        int StudentId,
        string StudentName,
        int LessonGroupId,
        int LessonId,
        string Subject,
        string GroupName,
        string TeacherName);
}
