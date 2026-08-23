using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Commands.RecordPayment;

public sealed class RecordPaymentCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<RecordPaymentCommand, Result<PaymentDto>>
{
    public async Task<Result<PaymentDto>> Handle(
        RecordPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<PaymentDto>.NotFound("Teacher profile was not found.");

        var lesson = await dbContext.Lessons
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.TeacherId == teacher.Id,
                cancellationToken);

        if (lesson is null)
            return Result<PaymentDto>.NotFound("الدرس غير موجود.");

        var student = await dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.StudentId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<PaymentDto>.NotFound("الطالب غير موجود.");

        var isOnLesson = await dbContext.LessonBookings.AnyAsync(
                x => x.LessonId == lesson.Id
                     && x.StudentId == student.Id
                     && x.Status == BookingStatus.Confirmed,
                cancellationToken)
            || await dbContext.LessonGroupMembers.AnyAsync(
                x => x.LessonGroup.LessonId == lesson.Id && x.StudentId == student.Id,
                cancellationToken);

        if (!isOnLesson)
            return Result<PaymentDto>.Failure("الطالب غير مرتبط بهذا الدرس.");

        var openChargesQuery = dbContext.Charges
            .AsTracking()
            .Where(x =>
                x.LessonId == lesson.Id
                && x.StudentId == student.Id
                && x.Status != ChargeStatus.Deferred
                && x.Allocations.Sum(a => a.Amount) < x.Amount);

        if (request.ChargeIds is { Count: > 0 })
            openChargesQuery = openChargesQuery.Where(x => request.ChargeIds.Contains(x.Id));

        var openCharges = await openChargesQuery
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (openCharges.Count == 0)
            return Result<PaymentDto>.Failure("لا توجد فواتير مفتوحة لتوزيع الدفعة عليها.");

        if (openCharges.Any(c => c.Id <= 0))
            return Result<PaymentDto>.Failure("فاتورة غير صالحة للتوزيع.");

        // Heal any drift where PaymentAllocations exist but Charge.AllocatedAmount lagged
        // (global NoTracking previously skipped charge UPDATEs).
        foreach (var charge in openCharges)
        {
            var allocated = await dbContext.PaymentAllocations
                .Where(a => a.ChargeId == charge.Id)
                .SumAsync(a => (decimal?)a.Amount, cancellationToken) ?? 0m;

            if (charge.AllocatedAmount != allocated)
            {
                charge.AllocatedAmount = allocated;
                charge.RecalculateStatus();
            }
        }

        openCharges = openCharges
            .Where(c => c.Status != ChargeStatus.Paid && c.AllocatedAmount < c.Amount)
            .ToList();

        if (openCharges.Count == 0)
            return Result<PaymentDto>.Failure("لا توجد فواتير مفتوحة لتوزيع الدفعة عليها.");

        var openRemaining = openCharges.Sum(c => c.Amount - c.AllocatedAmount);
        if (request.Amount > openRemaining)
            return Result<PaymentDto>.Failure($"المبلغ أكبر من المتبقي ({openRemaining:0.##}).");

        var nextReceipt = await dbContext.Payments
            .Where(x => x.TeacherId == teacher.Id)
            .Select(x => (int?)x.ReceiptNumber)
            .MaxAsync(cancellationToken) ?? 0;

        Payment payment;
        IReadOnlyList<PaymentAllocation> allocations;
        try
        {
            payment = Payment.Create(
                teacher.Id,
                student.Id,
                lesson.Id,
                request.Amount,
                request.Method,
                nextReceipt + 1,
                request.UserId,
                request.Note,
                request.PaidAtUtc);

            allocations = payment.AllocateFifo(openCharges);

            dbContext.Payments.Add(payment);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result<PaymentDto>.Failure(ex.Message);
        }

        await BillingNotifications.NotifyPaymentRecordedAsync(
            notificationService,
            payment,
            lesson.Subject,
            student.UserId,
            request.UserId,
            cancellationToken);

        return Result<PaymentDto>.Success(new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Method = payment.Method.ToString(),
            ReceiptNumber = payment.ReceiptNumber,
            PaidAtUtc = payment.PaidAtUtc,
            Note = payment.Note,
            Allocations = allocations.Select(a =>
            {
                var charge = openCharges.First(c => c.Id == a.ChargeId);
                return new PaymentAllocationDto
                {
                    ChargeId = a.ChargeId,
                    Amount = a.Amount,
                    ChargeType = charge.Type.ToString()
                };
            }).ToList()
        });
    }
}
