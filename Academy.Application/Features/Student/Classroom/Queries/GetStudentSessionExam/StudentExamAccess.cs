using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;

internal sealed record StudentExamAccessResult(int StudentId, LessonGroupSession Session);

internal static class StudentExamAccess
{
    public static async Task<Result<StudentExamAccessResult>> ResolveAsync(
        IApplicationDbContext dbContext,
        int userId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<StudentExamAccessResult>.NotFound("Student profile was not found.");

        var session = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

        if (session is null)
            return Result<StudentExamAccessResult>.NotFound("الحصة غير موجودة.");

        var isMember = await dbContext.LessonGroupMembers
            .AnyAsync(
                x => x.LessonGroupId == session.LessonGroupId && x.StudentId == student.Id,
                cancellationToken);

        if (!isMember)
            return Result<StudentExamAccessResult>.NotFound("الحصة غير موجودة.");

        if (!session.StartedAtUtc.HasValue)
            return Result<StudentExamAccessResult>.Conflict("لم يتم بدء الحصة بعد.");

        return Result<StudentExamAccessResult>.Success(new StudentExamAccessResult(student.Id, session));
    }
}
