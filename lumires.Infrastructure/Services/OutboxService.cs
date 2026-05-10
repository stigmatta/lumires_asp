using System.Text.Json;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Entities;

namespace Infrastructure.Services;

public sealed class OutboxService(IAppDbContext db) : INotificationService
{
    public async Task SendToUserAsync(Guid userId, NotificationMessage message)
    {
        await SaveOutboxAsync(userId, null, message);
    }

    public async Task SendToUsersAsync(Guid primaryUserId, Guid? secondaryUserId, NotificationMessage message)
    {
        await SaveOutboxAsync(primaryUserId, secondaryUserId, message);
    }

    private async Task SaveOutboxAsync(Guid primaryUserId, Guid? secondaryUserId, NotificationMessage message)
    {
        var outbox = new OutboxMessage(
            type: message.Type.ToString(),
            payload: JsonSerializer.Serialize(new OutboxPayload(primaryUserId, secondaryUserId, message))
        );

        await db.OutboxMessages.AddAsync(outbox);
    }
}

public sealed record OutboxPayload(
    Guid PrimaryUserId,      
    Guid? SecondaryUserId,   
    NotificationMessage Message
);