using System.Text.Json;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Entities;

namespace Infrastructure.Services;

public sealed class OutboxService(IAppDbContext db) : INotificationService
{
    public void SendToUsers(Guid primaryUserId, Guid? secondaryUserId, NotificationMessage message)
    {
        SaveOutbox(primaryUserId, secondaryUserId, message);
    }

    public void SendToUser(Guid userId, NotificationMessage message)
    {
        SaveOutbox(userId, null, message);
    }

    private void SaveOutbox(Guid primaryUserId, Guid? secondaryUserId, NotificationMessage message)
    {
        var outbox = new OutboxMessage(
            message.Type.ToString(),
            JsonSerializer.Serialize(new OutboxPayload(primaryUserId, secondaryUserId, message))
        );

        db.OutboxMessages.Add(outbox);
    }
}

public sealed record OutboxPayload(
    Guid PrimaryUserId,
    Guid? SecondaryUserId,
    NotificationMessage Message
);