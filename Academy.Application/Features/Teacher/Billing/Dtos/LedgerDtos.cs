using Academy.Application.Common.Images;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Billing.Dtos;

public sealed class TeacherBillingSummaryDto
{
    public required decimal ChargesTotal { get; init; }

    public required decimal PaymentsTotal { get; init; }

    public required decimal NetOutstanding { get; init; }

    public required decimal TodayChargesTotal { get; init; }

    public required decimal TodayPaymentsTotal { get; init; }

    public required decimal TodayNetOutstanding { get; init; }
}

public sealed class LedgerTransactionDto
{
    public required string Kind { get; init; }

    public required int Id { get; init; }

    public required DateTime OccurredAtUtc { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required int LessonId { get; init; }

    public required string LessonTitle { get; init; }

    public int? GroupId { get; init; }

    public string? GroupName { get; init; }

    public required decimal Amount { get; init; }

    public string? Type { get; init; }

    public int? ReceiptNumber { get; init; }

    public string? Method { get; init; }

    public string? Status { get; init; }

    public decimal? Remaining { get; init; }
}

public sealed class LedgerChargeRowDto
{
    public required int Id { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public string? PhotoUrl { get; init; }

    public required int LessonId { get; init; }

    public required string LessonTitle { get; init; }

    public int? GroupId { get; init; }

    public string? GroupName { get; init; }

    public required int AcademicYearId { get; init; }

    public required string AcademicYearName { get; init; }

    public required int EducationStageId { get; init; }

    public required string EducationStageName { get; init; }

    public DateOnly? SessionDate { get; init; }

    public TimeOnly? SessionStartTime { get; init; }

    public string? SessionTopic { get; init; }

    public required string Type { get; init; }

    public required decimal Amount { get; init; }

    public required decimal AllocatedAmount { get; init; }

    public required decimal Remaining { get; init; }

    public required string Status { get; init; }

    public required string Settlement { get; init; }

    public int? LessonGroupSessionId { get; init; }

    public DateOnly? CycleStartDate { get; init; }

    public DateOnly? CycleEndDate { get; init; }

    public string? Note { get; init; }
}

public sealed class LedgerChargeDetailDto
{
    public required LedgerChargeRowDto Charge { get; init; }

    public required IReadOnlyList<LedgerChargeAllocationDto> Allocations { get; init; }
}

public sealed class LedgerChargeAllocationDto
{
    public required int PaymentId { get; init; }

    public required int ReceiptNumber { get; init; }

    public required decimal Amount { get; init; }

    public required DateTime PaidAtUtc { get; init; }

    public required string Method { get; init; }
}

public sealed class LedgerPaymentRowDto
{
    public required int Id { get; init; }

    public required DateTime PaidAtUtc { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required int LessonId { get; init; }

    public required string LessonTitle { get; init; }

    public int? GroupId { get; init; }

    public string? GroupName { get; init; }

    public required decimal Amount { get; init; }

    public required string Method { get; init; }

    public required int ReceiptNumber { get; init; }

    public string? Note { get; init; }
}

public sealed class LedgerPaymentDetailDto
{
    public required LedgerPaymentRowDto Payment { get; init; }

    public required IReadOnlyList<PaymentAllocationDto> Allocations { get; init; }
}

public sealed class BillingStudentSearchDto
{
    public required int Id { get; init; }

    public required string FullName { get; init; }

    public string? StudentCode { get; init; }

    public string? PhoneNumber { get; init; }

    public string? PhotoUrl { get; init; }
}

public sealed class StudentOutstandingDto
{
    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public required decimal Remaining { get; init; }

    public required IReadOnlyList<StudentOutstandingLessonDto> Lessons { get; init; }
}

public sealed class StudentOutstandingLessonDto
{
    public required int LessonId { get; init; }

    public required string LessonTitle { get; init; }

    public required decimal Remaining { get; init; }

    public required IReadOnlyList<LedgerChargeRowDto> Charges { get; init; }
}

public sealed class LedgerFilterOptionDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }
}

public sealed class LedgerFilterSessionDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required DateOnly SessionDate { get; init; }

    public required TimeOnly StartTime { get; init; }

    public string? Topic { get; init; }

    public required bool IsMakeup { get; init; }
}

public sealed class RecordTeacherPaymentRequest
{
    public int StudentId { get; set; }

    public int LessonId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public string? Note { get; set; }

    public List<int>? ChargeIds { get; set; }

    public DateTime? PaidAtUtc { get; set; }
}

internal sealed class LedgerChargeSqlRow
{
    public int Id { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public string? Photo { get; init; }

    public int LessonId { get; init; }

    public required string LessonTitle { get; init; }

    public int? GroupId { get; init; }

    public string? GroupName { get; init; }

    public int AcademicYearId { get; init; }

    public required string AcademicYearName { get; init; }

    public int EducationStageId { get; init; }

    public required string EducationStageName { get; init; }

    public DateOnly? SessionDate { get; init; }

    public TimeOnly? SessionStartTime { get; init; }

    public string? SessionTopic { get; init; }

    public ChargeType Type { get; init; }

    public decimal Amount { get; init; }

    public decimal AllocatedAmount { get; init; }

    public ChargeStatus Status { get; init; }

    public ChargeSettlement Settlement { get; init; }

    public int? LessonGroupSessionId { get; init; }

    public DateOnly? CycleStartDate { get; init; }

    public DateOnly? CycleEndDate { get; init; }

    public string? Note { get; init; }
}

internal static class LedgerChargeRows
{
    public static IQueryable<LedgerChargeSqlRow> SelectRows(IQueryable<Domain.Entities.Charge> query) =>
        query.Select(x => new LedgerChargeSqlRow
        {
            Id = x.Id,
            CreatedAtUtc = x.CreatedAtUtc,
            StudentId = x.StudentId,
            StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
            StudentCode = x.Student.StudentCode,
            Photo = x.Student.User.ProfilePhoto,
            LessonId = x.LessonId,
            LessonTitle = x.Lesson.Subject,
            GroupId = x.LessonGroupId,
            GroupName = x.LessonGroup != null ? x.LessonGroup.Name : null,
            AcademicYearId = x.Lesson.AcademicYearId,
            AcademicYearName = x.Lesson.AcademicYear.Name,
            EducationStageId = x.Lesson.EducationStageId,
            EducationStageName = x.Lesson.EducationStage.NameAr,
            SessionDate = x.LessonGroupSession != null ? x.LessonGroupSession.SessionDate : null,
            SessionStartTime = x.LessonGroupSession != null ? x.LessonGroupSession.StartTime : null,
            SessionTopic = x.LessonGroupSession != null ? x.LessonGroupSession.Topic : null,
            Type = x.Type,
            Amount = x.Amount,
            AllocatedAmount = x.AllocatedAmount,
            Status = x.Status,
            Settlement = x.Settlement,
            LessonGroupSessionId = x.LessonGroupSessionId,
            CycleStartDate = x.CycleStartDate,
            CycleEndDate = x.CycleEndDate,
            Note = x.Note
        });

    public static LedgerChargeRowDto ToDto(LedgerChargeSqlRow row) =>
        new()
        {
            Id = row.Id,
            CreatedAtUtc = row.CreatedAtUtc,
            StudentId = row.StudentId,
            StudentName = row.StudentName,
            StudentCode = row.StudentCode,
            PhotoUrl = ImageService.DisplayValue(row.Photo),
            LessonId = row.LessonId,
            LessonTitle = row.LessonTitle,
            GroupId = row.GroupId,
            GroupName = row.GroupName,
            AcademicYearId = row.AcademicYearId,
            AcademicYearName = row.AcademicYearName,
            EducationStageId = row.EducationStageId,
            EducationStageName = row.EducationStageName,
            SessionDate = row.SessionDate,
            SessionStartTime = row.SessionStartTime,
            SessionTopic = row.SessionTopic,
            Type = row.Type.ToString(),
            Amount = row.Amount,
            AllocatedAmount = row.AllocatedAmount,
            Remaining = row.Amount - row.AllocatedAmount,
            Status = LedgerChargeStatus.Resolve(row.Status, row.Amount, row.AllocatedAmount),
            Settlement = row.Settlement.ToString(),
            LessonGroupSessionId = row.LessonGroupSessionId,
            CycleStartDate = row.CycleStartDate,
            CycleEndDate = row.CycleEndDate,
            Note = row.Note
        };
}
