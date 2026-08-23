using Academy.Application.Contracts.Ai;
using Academy.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Academy.Server.Notifications;

public sealed class SignalRExamGenerationProgress(IHubContext<NotificationsHub> hubContext)
    : IExamGenerationProgress
{
    public async Task ReportAsync(
        int userId,
        ExamGenerationProgressDto progress,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            step = progress.Step,
            current = progress.Current,
            total = progress.Total,
            percent = progress.Percent
        };

        var userIdText = userId.ToString();
        var group = NotificationsHub.UserGroup(userId);

        await hubContext.Clients.User(userIdText)
            .SendAsync("examGenerationProgress", payload, cancellationToken);

        await hubContext.Clients.Group(group)
            .SendAsync("examGenerationProgress", payload, cancellationToken);
    }
}
