using Academy.Domain.Entities;
using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Billing.Common;

internal static class LedgerQueryExtensions
{
    public static IQueryable<Charge> ApplyChargeFilters(
        this IQueryable<Charge> query,
        int? studentId,
        int? lessonId,
        int? groupId,
        DateTime? fromUtc,
        DateTime? toUtcExclusive,
        ChargeType? type,
        int? academicYearId = null,
        int? educationStageId = null,
        int? sessionId = null)
    {
        if (studentId is > 0)
            query = query.Where(x => x.StudentId == studentId.Value);

        if (academicYearId is > 0)
            query = query.Where(x => x.Lesson.AcademicYearId == academicYearId.Value);

        if (educationStageId is > 0)
            query = query.Where(x => x.Lesson.EducationStageId == educationStageId.Value);

        if (lessonId is > 0)
            query = query.Where(x => x.LessonId == lessonId.Value);

        if (groupId is > 0)
            query = query.Where(x => x.LessonGroupId == groupId.Value);

        if (sessionId is > 0)
            query = query.Where(x => x.LessonGroupSessionId == sessionId.Value);

        if (fromUtc is DateTime from)
            query = query.Where(x => x.CreatedAtUtc >= from);

        if (toUtcExclusive is DateTime to)
            query = query.Where(x => x.CreatedAtUtc < to);

        if (type is ChargeType chargeType)
            query = query.Where(x => x.Type == chargeType);

        return query;
    }

    public static IQueryable<Payment> ApplyPaymentFilters(
        this IQueryable<Payment> query,
        int? studentId,
        int? lessonId,
        int? groupId,
        DateTime? fromUtc,
        DateTime? toUtcExclusive,
        int? academicYearId = null,
        int? educationStageId = null,
        int? sessionId = null)
    {
        if (studentId is > 0)
            query = query.Where(x => x.StudentId == studentId.Value);

        if (academicYearId is > 0)
            query = query.Where(x => x.Lesson.AcademicYearId == academicYearId.Value);

        if (educationStageId is > 0)
            query = query.Where(x => x.Lesson.EducationStageId == educationStageId.Value);

        if (lessonId is > 0)
            query = query.Where(x => x.LessonId == lessonId.Value);

        if (groupId is > 0)
            query = query.Where(x => x.Allocations.Any(a => a.Charge.LessonGroupId == groupId.Value));

        if (sessionId is > 0)
            query = query.Where(x => x.Allocations.Any(a => a.Charge.LessonGroupSessionId == sessionId.Value));

        if (fromUtc is DateTime from)
            query = query.Where(x => x.PaidAtUtc >= from);

        if (toUtcExclusive is DateTime to)
            query = query.Where(x => x.PaidAtUtc < to);

        return query;
    }
}
