using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Academy.Server.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub
{
    public static string UserGroup(int userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var raw =
            Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub")
            ?? Context.UserIdentifier;

        if (int.TryParse(raw, out var id) && id > 0)
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(id));

        await base.OnConnectedAsync();
    }
}
