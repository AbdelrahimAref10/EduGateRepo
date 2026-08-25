using Academy.Application.Contracts.Persistence;
using Academy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Students.Common;

internal static class TeacherStudentAccess
{
    public static async Task<int?> GetTeacherIdAsync(
        IApplicationDbContext dbContext,
        int userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<bool> IsTeachersConfirmedStudentAsync(
        IApplicationDbContext dbContext,
        int teacherId,
        int studentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.LessonBookings
            .AsNoTracking()
            .AnyAsync(
                x => x.TeacherId == teacherId
                     && x.StudentId == studentId
                     && x.Status == BookingStatus.Confirmed
                     && !x.Student.IsParent,
                cancellationToken);
    }

    public static async Task<bool> OwnsLessonAsync(
        IApplicationDbContext dbContext,
        int teacherId,
        int lessonId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Lessons
            .AsNoTracking()
            .AnyAsync(x => x.Id == lessonId && x.TeacherId == teacherId, cancellationToken);
    }
}
