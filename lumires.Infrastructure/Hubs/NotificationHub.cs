using Contracts.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

[Authorize]
internal sealed class NotificationHub(ICurrentUserService currentUser) : Hub<INotificationClient>
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