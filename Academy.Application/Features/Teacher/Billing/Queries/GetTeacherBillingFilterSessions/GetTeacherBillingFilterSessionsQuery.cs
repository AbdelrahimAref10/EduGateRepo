using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterSessions;

public sealed record GetTeacherBillingFilterSessionsQuery(int UserId, int GroupId)
    : IRequest<Result<IReadOnlyList<LedgerFilterSessionDto>>>;

public sealed class GetTeacherBillingFilterSessionsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingFilterSessionsQuery, Result<IReadOnlyList<LedgerFilterSessionDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerFilterSessionDto>>> Handle(
        GetTeacherBillingFilterSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LedgerFilterSessionDto>>.NotFound("Teacher profile was not found.");

        var groupOk = await dbContext.LessonGroups.AnyAsync(
            x => x.Id == request.GroupId && x.Lesson.TeacherId == teacherId.Value,
            cancellationToken);

        if (!groupOk)
            return Result<IReadOnlyList<LedgerFilterSessionDto>>.NotFound("المجموعة غير موجودة.");

        var items = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.LessonGroupId == request.GroupId)
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.SessionDate,
                x.StartTime,
                x.Topic,
                x.IsMakeup
            })
            .ToListAsync(cancellationToken);

        var mapped = items
            .Select(x =>
            {
                var topic = string.IsNullOrWhiteSpace(x.Topic) ? null : x.Topic.Trim();
                var name = $"{x.SessionDate:yyyy-MM-dd} {x.StartTime:HH\\:mm}";
                if (topic is not null)
                    name = $"{name} — {topic}";
                if (x.IsMakeup)
                    name = $"{name} (تعويض)";

                return new LedgerFilterSessionDto
                {
                    Id = x.Id,
                    Name = name,
                    SessionDate = x.SessionDate,
                    StartTime = x.StartTime,
                    Topic = topic,
                    IsMakeup = x.IsMakeup
                };
            })
            .ToList();

        return Result<IReadOnlyList<LedgerFilterSessionDto>>.Success(mapped);
    }
}
