using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingTransactions;

public sealed record GetTeacherBillingTransactionsQuery(
    int UserId,
    int? StudentId = null,
    int? AcademicYearId = null,
    int? EducationStageId = null,
    int? LessonId = null,
    int? GroupId = null,
    int? SessionId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    ChargeType? Type = null,
    string? Kind = null,
    int Page = 1,
    int PageSize = LedgerPaging.PageSize)
    : IRequest<Result<PagedResult<LedgerTransactionDto>>>;

public sealed class GetTeacherBillingTransactionsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingTransactionsQuery, Result<PagedResult<LedgerTransactionDto>>>
{
    public async Task<Result<PagedResult<LedgerTransactionDto>>> Handle(
        GetTeacherBillingTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = LedgerPaging.Normalize(request.Page, request.PageSize);

        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<PagedResult<LedgerTransactionDto>>.NotFound("Teacher profile was not found.");

        var (fromUtc, toUtc) = LedgerCalendar.Range(request.From, request.To);
        var kind = request.Kind?.Trim();
        var includeCharges = kind is null or "" || kind.Equals("Charge", StringComparison.OrdinalIgnoreCase);
        var includePayments =
            (kind is null or "" || kind.Equals("Payment", StringComparison.OrdinalIgnoreCase))
            && request.Type is null;

        var charges = dbContext.Charges
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .ApplyChargeFilters(
                request.StudentId,
                request.LessonId,
                request.GroupId,
                fromUtc,
                toUtc,
                request.Type,
                request.AcademicYearId,
                request.EducationStageId,
                request.SessionId);

        var payments = dbContext.Payments
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .ApplyPaymentFilters(
                request.StudentId,
                request.LessonId,
                request.GroupId,
                fromUtc,
                toUtc,
                request.AcademicYearId,
                request.EducationStageId,
                request.SessionId);

        var chargeCount = includeCharges ? await charges.CountAsync(cancellationToken) : 0;
        var paymentCount = includePayments ? await payments.CountAsync(cancellationToken) : 0;
        var totalCount = chargeCount + paymentCount;

        if (totalCount == 0)
            return Result<PagedResult<LedgerTransactionDto>>.Success(
                PagedResult<LedgerTransactionDto>.Empty(page, pageSize));

        var window = skip + pageSize;

        var chargeKeys = includeCharges && chargeCount > 0
            ? (await charges
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id)
                .Select(x => new { x.Id, x.CreatedAtUtc })
                .Take(window)
                .ToListAsync(cancellationToken))
                .Select(x => new LedgerEntryKey
                {
                    Kind = 1,
                    Id = x.Id,
                    OccurredAtUtc = x.CreatedAtUtc
                })
            : [];

        var paymentKeys = includePayments && paymentCount > 0
            ? (await payments
                .OrderByDescending(x => x.PaidAtUtc)
                .ThenByDescending(x => x.Id)
                .Select(x => new { x.Id, x.PaidAtUtc })
                .Take(window)
                .ToListAsync(cancellationToken))
                .Select(x => new LedgerEntryKey
                {
                    Kind = 2,
                    Id = x.Id,
                    OccurredAtUtc = x.PaidAtUtc
                })
            : [];

        var pageKeys = chargeKeys
            .Concat(paymentKeys)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Kind)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        var chargeIds = pageKeys.Where(x => x.Kind == 1).Select(x => x.Id).ToList();
        var paymentIds = pageKeys.Where(x => x.Kind == 2).Select(x => x.Id).ToList();

        var chargeRows = chargeIds.Count == 0
            ? []
            : await dbContext.Charges
                .AsNoTracking()
                .Where(x => chargeIds.Contains(x.Id))
                .Select(x => new LedgerTransactionSqlRow
                {
                    Kind = 1,
                    Id = x.Id,
                    OccurredAtUtc = x.CreatedAtUtc,
                    StudentId = x.StudentId,
                    StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                    StudentCode = x.Student.StudentCode,
                    LessonId = x.LessonId,
                    LessonTitle = x.Lesson.Subject,
                    GroupId = x.LessonGroupId,
                    GroupName = x.LessonGroup != null ? x.LessonGroup.Name : null,
                    Amount = x.Amount,
                    AllocatedAmount = x.AllocatedAmount,
                    ChargeStatus = x.Status,
                    ChargeType = x.Type,
                    ReceiptNumber = null,
                    Method = null
                })
                .ToListAsync(cancellationToken);

        var paymentRows = paymentIds.Count == 0
            ? []
            : await dbContext.Payments
                .AsNoTracking()
                .Where(x => paymentIds.Contains(x.Id))
                .Select(x => new LedgerTransactionSqlRow
                {
                    Kind = 2,
                    Id = x.Id,
                    OccurredAtUtc = x.PaidAtUtc,
                    StudentId = x.StudentId,
                    StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                    StudentCode = x.Student.StudentCode,
                    LessonId = x.LessonId,
                    LessonTitle = x.Lesson.Subject,
                    GroupId = null,
                    GroupName = null,
                    Amount = x.Amount,
                    AllocatedAmount = 0m,
                    ChargeStatus = null,
                    ChargeType = null,
                    ReceiptNumber = x.ReceiptNumber,
                    Method = x.Method
                })
                .ToListAsync(cancellationToken);

        var byKey = chargeRows.Concat(paymentRows).ToDictionary(x => (x.Kind, x.Id));
        var items = pageKeys
            .Select(key => byKey.TryGetValue((key.Kind, key.Id), out var row) ? Map(row) : null)
            .Where(x => x is not null)
            .Cast<LedgerTransactionDto>()
            .ToList();

        return Result<PagedResult<LedgerTransactionDto>>.Success(
            PagedResult<LedgerTransactionDto>.Create(items, totalCount, page, pageSize));
    }

    private static LedgerTransactionDto Map(LedgerTransactionSqlRow row)
    {
        var isCharge = row.Kind == 1;
        return new LedgerTransactionDto
        {
            Kind = isCharge ? "Charge" : "Payment",
            Id = row.Id,
            OccurredAtUtc = row.OccurredAtUtc,
            StudentId = row.StudentId,
            StudentName = row.StudentName,
            StudentCode = row.StudentCode,
            LessonId = row.LessonId,
            LessonTitle = row.LessonTitle,
            GroupId = row.GroupId,
            GroupName = row.GroupName,
            Amount = row.Amount,
            Type = row.ChargeType?.ToString(),
            ReceiptNumber = row.ReceiptNumber,
            Method = row.Method?.ToString(),
            Status = isCharge && row.ChargeStatus is ChargeStatus status
                ? LedgerChargeStatus.Resolve(status, row.Amount, row.AllocatedAmount)
                : null,
            Remaining = isCharge ? row.Amount - row.AllocatedAmount : null
        };
    }
}

internal sealed class LedgerEntryKey
{
    public int Kind { get; set; }

    public int Id { get; set; }

    public DateTime OccurredAtUtc { get; set; }
}

internal sealed class LedgerTransactionSqlRow
{
    public int Kind { get; init; }

    public int Id { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public int LessonId { get; init; }

    public required string LessonTitle { get; init; }

    public int? GroupId { get; init; }

    public string? GroupName { get; init; }

    public decimal Amount { get; init; }

    public decimal AllocatedAmount { get; init; }

    public ChargeStatus? ChargeStatus { get; init; }

    public ChargeType? ChargeType { get; init; }

    public int? ReceiptNumber { get; init; }

    public PaymentMethod? Method { get; init; }
}
