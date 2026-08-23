using Academy.Application.Common.Models;
using Academy.Application.Contracts.Notifications;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Application.Features.Teacher.Lessons;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Commands.CreateMakeupSession;

public sealed class CreateMakeupSessionCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notificationService)
    : IRequestHandler<CreateMakeupSessionCommand, Result<LessonGroupSessionDto>>
{
    public async Task<Result<LessonGroupSessionDto>> Handle(
        CreateMakeupSessionCommand request,
        CancellationToken cancellationToken)
    {
        var group = await dbContext.LessonGroups
            .Include(x => x.Lesson)
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId
                     && x.LessonId == request.LessonId
                     && x.Lesson.Teacher.UserId == request.UserId,
                cancellationToken);

        if (group is null)
            return Result<LessonGroupSessionDto>.NotFound("المجموعة غير موجودة.");

        if (group.EndedAtUtc.HasValue)
            return Result<LessonGroupSessionDto>.Conflict("المجموعة منتهية.");

        var studentIds = request.StudentIds.Distinct().ToList();
        var members = await dbContext.LessonGroupMembers
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Where(x => x.LessonGroupId == group.Id && studentIds.Contains(x.StudentId))
            .ToListAsync(cancellationToken);

        if (members.Count != studentIds.Count)
            return Result<LessonGroupSessionDto>.Failure("بعض الطلبة غير أعضاء في هذه المجموعة.");

        if (request.MakeupForSessionId is int originId)
        {
            var originOk = await dbContext.LessonGroupSessions.AnyAsync(
                x => x.Id == originId && x.LessonGroupId == group.Id && !x.IsMakeup,
                cancellationToken);

            if (!originOk)
                return Result<LessonGroupSessionDto>.Failure("الحصة الأصلية غير صالحة.");
        }

        var clash = await dbContext.LessonGroupSessions.AnyAsync(
            x => x.LessonGroupId == group.Id
                 && x.SessionDate == request.SessionDate
                 && x.StartTime == request.StartTime,
            cancellationToken);

        if (clash)
            return Result<LessonGroupSessionDto>.Conflict("يوجد حصة في نفس الموعد.");

        var lesson = group.Lesson;
        ChargeSettlement settlement;
        decimal? amount;

        if (request.IsFree)
        {
            settlement = ChargeSettlement.None;
            amount = null;
        }
        else if (lesson.IsPerSession)
        {
            // Immediate makeup debt at session price — no monthly settlement UI.
            settlement = ChargeSettlement.Standalone;
            amount = lesson.SessionPrice;
            if (amount is null or <= 0)
                return Result<LessonGroupSessionDto>.Failure("سعر الحصة غير مضبوط على الدرس.");
        }
        else
        {
            // Monthly: teacher chooses how the paid makeup lands on the ledger.
            settlement = request.Settlement switch
            {
                ChargeSettlement.CurrentCycle => ChargeSettlement.CurrentCycle,
                ChargeSettlement.NextCycle => ChargeSettlement.NextCycle,
                _ => ChargeSettlement.Standalone
            };
            amount = request.Amount;
            if (amount is null or <= 0)
                return Result<LessonGroupSessionDto>.Failure("حدد مبلغ التعويض.");
        }

        var session = LessonGroupSession.CreateMakeup(
            group.Id,
            request.SessionDate,
            request.StartTime,
            request.Topic,
            request.MakeupForSessionId);

        dbContext.LessonGroupSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        await ClassroomSeeding.EnsureInvitedStudentDetailsAsync(
            dbContext,
            session,
            studentIds,
            cancellationToken);

        var createdCharges = new List<Charge>();
        if (!request.IsFree && amount is > 0)
        {
            foreach (var member in members)
            {
                Charge? currentCycle = null;
                if (settlement == ChargeSettlement.CurrentCycle)
                {
                    var cycles = await dbContext.Charges
                        .Where(x =>
                            x.LessonId == lesson.Id
                            && x.StudentId == member.StudentId
                            && x.Type == ChargeType.MonthlyCycle)
                        .ToListAsync(cancellationToken);

                    currentCycle = cycles.FirstOrDefault(c => c.CoversDate(session.SessionDate));
                }

                try
                {
                    var charge = Charge.CreateMakeupCharge(
                        lesson,
                        session,
                        member.StudentId,
                        amount.Value,
                        settlement,
                        currentCycle,
                        request.UserId);

                    dbContext.Charges.Add(charge);
                    createdCharges.Add(charge);
                }
                catch (InvalidOperationException ex)
                {
                    return Result<LessonGroupSessionDto>.Failure(ex.Message);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var userIds = members.Select(m => m.Student.UserId).Where(id => id > 0).Distinct().ToList();
        await BillingNotifications.NotifyMakeupScheduledAsync(
            notificationService,
            session,
            lesson.Subject,
            userIds,
            cancellationToken);

        foreach (var charge in createdCharges)
        {
            var member = members.First(m => m.StudentId == charge.StudentId);
            if (member.Student.UserId > 0)
            {
                await BillingNotifications.NotifyChargeCreatedAsync(
                    notificationService,
                    charge,
                    member.Student.User.FullName,
                    lesson.Subject,
                    member.Student.UserId,
                    cancellationToken);
            }
        }

        return Result<LessonGroupSessionDto>.Success(
            LessonMappings.ToSessionDto(session, group.EndedAtUtc.HasValue));
    }
}
