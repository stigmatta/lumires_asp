using lumires.Api.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace lumires.Api.Hubs;

[Authorize]
public class NotificationHub(ICurrentUserService currentUser) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = currentUser.UserId;

        if (userId != Guid.Empty)
        {
            var userGroup = $"user_{userId:D}"; 
            await Groups.AddToGroupAsync(Context.ConnectionId, userGroup);
        }

        await base.OnConnectedAsync();
    }
}