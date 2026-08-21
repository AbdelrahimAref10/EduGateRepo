using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    public async Task<Result> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        var unread = await dbContext.NotificationDetails
            .AsTracking()
            .Where(x => x.UserId == request.UserId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
            return Result.Success();

        foreach (var item in unread)
            item.IsRead = true;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
