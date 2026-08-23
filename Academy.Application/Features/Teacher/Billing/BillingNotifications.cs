using Academy.Application.Contracts.Notifications;
using Academy.Domain.Entities;
using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Billing;

internal static class BillingNotifications
{
    public static Task NotifyChargeCreatedAsync(
        INotificationService notifications,
        Charge charge,
        string studentName,
        string subject,
        int studentUserId,
        CancellationToken cancellationToken)
    {
        var typeLabelAr = charge.Type switch
        {
            ChargeType.Session => "حصة",
            ChargeType.MonthlyCycle => "اشتراك شهري",
            ChargeType.Makeup => "حصة تعويض",
            _ => "فاتورة"
        };
        var typeLabelEn = charge.Type switch
        {
            ChargeType.Session => "session",
            ChargeType.MonthlyCycle => "monthly cycle",
            ChargeType.Makeup => "makeup session",
            _ => "charge"
        };

        return notifications.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [studentUserId],
                UserTargetId = studentUserId,
                Type = NotificationType.ChargeCreated,
                EntityType = NotificationEntityType.Charge,
                EntityId = charge.Id,
                TitleAr = "فاتورة جديدة",
                TitleEn = "New charge",
                BodyAr = $"تم تسجيل استحقاق ({typeLabelAr}) بمبلغ {charge.Amount:0.##} على درس {subject}.",
                BodyEn = $"A {typeLabelEn} charge of {charge.Amount:0.##} was added for {subject}.",
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }

    public static Task NotifyPaymentRecordedAsync(
        INotificationService notifications,
        Payment payment,
        string subject,
        int studentUserId,
        int teacherUserId,
        CancellationToken cancellationToken)
    {
        return notifications.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = [studentUserId],
                UserTargetId = teacherUserId,
                Type = NotificationType.PaymentRecorded,
                EntityType = NotificationEntityType.Payment,
                EntityId = payment.Id,
                TitleAr = "تم تسجيل دفعة",
                TitleEn = "Payment recorded",
                BodyAr = $"تم تسجيل دفعة بمبلغ {payment.Amount:0.##} لدرس {subject}. رقم الإيصال: {payment.ReceiptNumber}. يمكنك تحميل إيصال PDF.",
                BodyEn = $"A payment of {payment.Amount:0.##} was recorded for {subject}. Receipt #{payment.ReceiptNumber}. You can download the PDF receipt.",
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }

    public static Task NotifyMakeupScheduledAsync(
        INotificationService notifications,
        LessonGroupSession session,
        string subject,
        IReadOnlyList<int> studentUserIds,
        CancellationToken cancellationToken)
    {
        if (studentUserIds.Count == 0)
            return Task.CompletedTask;

        return notifications.CreateAsync(
            new NotificationCreateRequest
            {
                RecipientUserIds = studentUserIds,
                Type = NotificationType.MakeupSessionScheduled,
                EntityType = NotificationEntityType.Session,
                EntityId = session.Id,
                TitleAr = "حصة تعويض",
                TitleEn = "Makeup session",
                BodyAr = $"تم جدولة حصة تعويض لدرس {subject} بتاريخ {session.SessionDate:yyyy-MM-dd}.",
                BodyEn = $"A makeup session for {subject} was scheduled on {session.SessionDate:yyyy-MM-dd}.",
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }
}
