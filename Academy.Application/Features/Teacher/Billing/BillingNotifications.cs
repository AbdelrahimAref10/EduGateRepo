using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent;
using Academy.Domain.Entities;
using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Billing;

internal static class BillingNotifications
{
    public static async Task NotifyChargeCreatedAsync(
        IApplicationDbContext dbContext,
        INotificationService notifications,
        Charge charge,
        string studentName,
        string subject,
        int studentUserId,
        int studentId,
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

        await notifications.CreateAsync(
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

        await ParentNotifications.NotifyLinkedParentsAsync(
            dbContext,
            notifications,
            studentId,
            new NotificationCreateRequest
            {
                RecipientUserIds = [],
                UserTargetId = studentUserId,
                Type = NotificationType.ChargeCreated,
                EntityType = NotificationEntityType.Charge,
                EntityId = charge.Id,
                TitleAr = "فاتورة جديدة لابنك/ابنتك",
                TitleEn = "New charge for your child",
                BodyAr = $"تم تسجيل استحقاق ({typeLabelAr}) بمبلغ {charge.Amount:0.##} على درس {subject} للطالب {studentName}.",
                BodyEn = $"A {typeLabelEn} charge of {charge.Amount:0.##} was added for {studentName} on '{subject}'.",
                IncludeSuperAdmins = false
            },
            cancellationToken);
    }

    public static async Task NotifyPaymentRecordedAsync(
        IApplicationDbContext dbContext,
        INotificationService notifications,
        Payment payment,
        string subject,
        int studentUserId,
        int studentId,
        string studentName,
        int teacherUserId,
        CancellationToken cancellationToken)
    {
        await notifications.CreateAsync(
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

        await ParentNotifications.NotifyLinkedParentsAsync(
            dbContext,
            notifications,
            studentId,
            new NotificationCreateRequest
            {
                RecipientUserIds = [],
                UserTargetId = teacherUserId,
                Type = NotificationType.PaymentRecorded,
                EntityType = NotificationEntityType.Payment,
                EntityId = payment.Id,
                TitleAr = "تم سداد دفعة",
                TitleEn = "Payment recorded for your child",
                BodyAr = $"تم تسجيل دفعة بمبلغ {payment.Amount:0.##} لدرس {subject} للطالب {studentName}. رقم الإيصال: {payment.ReceiptNumber}.",
                BodyEn = $"A payment of {payment.Amount:0.##} was recorded for {studentName} on '{subject}'. Receipt #{payment.ReceiptNumber}.",
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
