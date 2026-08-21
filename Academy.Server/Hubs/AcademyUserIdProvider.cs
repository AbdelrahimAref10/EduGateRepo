using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Academy.Server.Hubs;

public sealed class AcademyUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var user = connection.User;
        if (user is null)
            return null;

        return user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue("sub")
               ?? user.FindFirstValue("nameid");
    }
}
