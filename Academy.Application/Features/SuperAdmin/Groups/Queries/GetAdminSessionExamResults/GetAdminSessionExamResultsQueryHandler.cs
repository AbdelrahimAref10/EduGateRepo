using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminSessionExamResults;

public sealed class GetAdminSessionExamResultsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAdminSessionExamResultsQuery, Result<TeacherExamResultsDto>>
{
    public async Task<Result<TeacherExamResultsDto>> Handle(
        GetAdminSessionExamResultsQuery request,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.Id == request.SessionId)
            .Select(x => new { x.Id, x.LessonGroupId, x.StartedAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            return Result<TeacherExamResultsDto>.NotFound("الحصة غير موجودة.");

        if (session.StartedAtUtc is null)
            return Result<TeacherExamResultsDto>.Conflict("لم يتم بدء الحصة بعد.");

        var exam = await dbContext.Exams
            .AsNoTracking()
            .Where(x => x.LessonGroupSessionId == request.SessionId)
            .Select(x => new { x.Id, x.Title, Status = (int)x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            return Result<TeacherExamResultsDto>.NotFound("لا يوجد امتحان لهذه الحصة.");

        var members = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .OrderBy(x => x.AddedAtUtc)
            .Select(x => new RosterStudent(
                x.StudentId,
                x.Student.User.FirstName + " " + x.Student.User.LastName,
                x.Student.StudentCode))
            .ToListAsync(cancellationToken);

        var sessionStudents = await dbContext.LessonSessionStudentDetails
            .AsNoTracking()
            .Where(x => x.LessonGroupSessionId == session.Id)
            .Select(x => new RosterStudent(
                x.StudentId,
                x.Student.User.FirstName + " " + x.Student.User.LastName,
                x.Student.StudentCode))
            .ToListAsync(cancellationToken);

        var attempts = await dbContext.ExamAttempts
            .AsNoTracking()
            .Where(x => x.ExamId == exam.Id)
            .Select(x => new AttemptRow(
                x.StudentId,
                x.Student.User.FirstName + " " + x.Student.User.LastName,
                x.Student.StudentCode,
                x.Score,
                x.MaxScore,
                x.SubmittedAtUtc))
            .ToListAsync(cancellationToken);

        var attemptByStudentId = attempts
            .GroupBy(x => x.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.SubmittedAtUtc.HasValue)
                    .ThenByDescending(a => a.SubmittedAtUtc)
                    .First());

        var roster = new List<RosterStudent>(members.Count + sessionStudents.Count);
        var seen = new HashSet<int>();

        foreach (var person in members.Concat(sessionStudents))
        {
            if (seen.Add(person.StudentId))
                roster.Add(person);
        }

        foreach (var attempt in attemptByStudentId.Values)
        {
            if (seen.Add(attempt.StudentId))
                roster.Add(new RosterStudent(attempt.StudentId, attempt.StudentName.Trim(), attempt.StudentCode));
        }

        var rows = roster.Select(person =>
        {
            attemptByStudentId.TryGetValue(person.StudentId, out var attempt);
            var submitted = attempt?.SubmittedAtUtc is not null;
            return new TeacherExamResultRowDto
            {
                StudentId = person.StudentId,
                StudentName = person.StudentName.Trim(),
                StudentCode = person.StudentCode,
                HasSubmitted = submitted,
                Score = submitted ? attempt!.Score : null,
                MaxScore = submitted ? attempt!.MaxScore : null,
                Percentage = submitted && attempt!.MaxScore > 0
                    ? Math.Round(attempt.Score * 100m / attempt.MaxScore, 1)
                    : null,
                SubmittedAtUtc = attempt?.SubmittedAtUtc,
                Questions = []
            };
        }).ToList();

        return Result<TeacherExamResultsDto>.Success(new TeacherExamResultsDto
        {
            ExamId = exam.Id,
            Title = exam.Title,
            Status = exam.Status,
            SubmittedCount = rows.Count(x => x.HasSubmitted),
            StudentCount = rows.Count,
            Students = rows
        });
    }

    private sealed record RosterStudent(int StudentId, string StudentName, string? StudentCode);

    private sealed record AttemptRow(
        int StudentId,
        string StudentName,
        string? StudentCode,
        int Score,
        int MaxScore,
        DateTime? SubmittedAtUtc);
}
