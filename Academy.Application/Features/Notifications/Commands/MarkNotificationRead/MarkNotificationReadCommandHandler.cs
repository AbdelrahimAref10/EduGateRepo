using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Notifications.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public async Task<Result> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        var detail = await dbContext.NotificationDetails
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.NotificationId && x.UserId == request.UserId,
                cancellationToken);

        if (detail is null)
            return Result.NotFound("Notification was not found.");

        if (!detail.IsRead)
        {
            detail.IsRead = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
