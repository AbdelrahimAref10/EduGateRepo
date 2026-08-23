using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Application.Features.Teacher.Billing;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateStudentSessionDetail;

public sealed class UpdateStudentSessionDetailCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<UpdateStudentSessionDetailCommand, Result<ClassroomStudentDetailDto>>
{
    public async Task<Result<ClassroomStudentDetailDto>> Handle(
        UpdateStudentSessionDetailCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<ClassroomStudentDetailDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken,
            asTracking: true);

        if (session is null)
            return Result<ClassroomStudentDetailDto>.NotFound("الحصة غير موجودة.");

        if (!session.IsMakeup)
            await ClassroomSeeding.EnsureStudentDetailsAsync(dbContext, session, cancellationToken);

        var isMember = session.IsMakeup
            ? await dbContext.LessonSessionStudentDetails.AnyAsync(
                x => x.LessonGroupSessionId == session.Id && x.StudentId == request.StudentId,
                cancellationToken)
            : await dbContext.LessonGroupMembers.AnyAsync(
                x => x.LessonGroupId == session.LessonGroupId && x.StudentId == request.StudentId,
                cancellationToken);

        if (!isMember)
            return Result<ClassroomStudentDetailDto>.NotFound("الطالب غير موجود في هذه المجموعة.");

        var detail = await dbContext.LessonSessionStudentDetails
            .AsTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.LessonGroupSessionId == request.SessionId && x.StudentId == request.StudentId,
                cancellationToken);

        if (detail is null)
        {
            detail = new LessonSessionStudentDetail
            {
                LessonGroupSessionId = request.SessionId,
                StudentId = request.StudentId,
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.LessonSessionStudentDetails.Add(detail);
        }

        detail.IsPresent = request.IsPresent;
        detail.TeacherNotes = string.IsNullOrWhiteSpace(request.TeacherNotes)
            ? null
            : request.TeacherNotes.Trim();
        detail.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var lesson = session.LessonGroup.Lesson;
        Charge? createdCharge = null;

        try
        {
            createdCharge = await SyncChargesForAttendanceAsync(
                session,
                lesson,
                request.StudentId,
                request.IsPresent,
                request.UserId,
                cancellationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("دفعات") || ex.Message.Contains("payment", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ClassroomStudentDetailDto>.Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Result<ClassroomStudentDetailDto>.Failure(ex.Message);
        }

        if (createdCharge is not null && detail.Student.UserId > 0)
        {
            await BillingNotifications.NotifyChargeCreatedAsync(
                notificationService,
                createdCharge,
                detail.Student.User.FullName,
                lesson.Subject,
                detail.Student.UserId,
                cancellationToken);
        }

        if (detail.Student?.User is null)
        {
            detail = await dbContext.LessonSessionStudentDetails
                .AsNoTracking()
                .Include(x => x.Student)
                    .ThenInclude(x => x.User)
                .FirstAsync(x => x.Id == detail.Id, cancellationToken);
        }

        var hintCharges = await ClassroomChargeQuery.ForStudentAsync(
            dbContext,
            lesson,
            session,
            request.StudentId,
            cancellationToken);
        var (outstanding, status) = Charge.Summarize(hintCharges);

        return Result<ClassroomStudentDetailDto>.Success(
            ClassroomMappings.ToStudentDetailDto(detail, outstanding, status));
    }

    private async Task<Charge?> SyncChargesForAttendanceAsync(
        LessonGroupSession session,
        Lesson lesson,
        int studentId,
        bool isPresent,
        int userId,
        CancellationToken cancellationToken)
    {
        if (session.IsMakeup)
            return null;

        if (lesson.IsPerSession)
            return await SyncPerSessionAsync(session, lesson, studentId, isPresent, userId, cancellationToken);

        if (lesson.IsMonthly && isPresent)
            return await SyncMonthlyAsync(session, lesson, studentId, userId, cancellationToken);

        return null;
    }

    private async Task<Charge?> SyncPerSessionAsync(
        LessonGroupSession session,
        Lesson lesson,
        int studentId,
        bool isPresent,
        int userId,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.Charges
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.LessonGroupSessionId == session.Id
                     && x.StudentId == studentId
                     && x.Type == ChargeType.Session,
                cancellationToken);

        if (!lesson.ShouldCreateSessionCharge(isPresent))
        {
            if (existing is null)
                return null;

            if (!existing.CanBeRemoved)
            {
                throw new InvalidOperationException(
                    "لا يمكن إلغاء الحضور/الغياب المحاسَب لأن هناك دفعات مسجّلة على هذه الفاتورة.");
            }

            dbContext.Charges.Remove(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (existing is not null)
            return null;

        var charge = Charge.CreateSessionCharge(lesson, session, studentId, userId);
        dbContext.Charges.Add(charge);
        await dbContext.SaveChangesAsync(cancellationToken);
        return charge;
    }

    private async Task<Charge?> SyncMonthlyAsync(
        LessonGroupSession session,
        Lesson lesson,
        int studentId,
        int userId,
        CancellationToken cancellationToken)
    {
        var active = await dbContext.Charges
            .AsTracking()
            .Where(x =>
                x.LessonId == lesson.Id
                && x.StudentId == studentId
                && x.Type == ChargeType.MonthlyCycle)
            .ToListAsync(cancellationToken);

        if (active.Any(c => c.CoversDate(session.SessionDate)))
            return null;

        var charge = Charge.CreateMonthlyCycle(lesson, session, studentId, userId);
        dbContext.Charges.Add(charge);
        await dbContext.SaveChangesAsync(cancellationToken);

        var deferred = await dbContext.Charges
            .AsTracking()
            .Where(x =>
                x.LessonId == lesson.Id
                && x.StudentId == studentId
                && x.Type == ChargeType.Makeup
                && x.Status == ChargeStatus.Deferred
                && x.Settlement == ChargeSettlement.NextCycle)
            .ToListAsync(cancellationToken);

        foreach (var makeup in deferred)
            makeup.ActivateDeferredAgainstCycle(charge);

        if (deferred.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return charge;
    }
}
