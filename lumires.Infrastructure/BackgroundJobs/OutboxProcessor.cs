using System.Text.Json;
using Infrastructure.Services;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await ProcessAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        var payloads = messages
            .Select(m => (message: m, payload: JsonSerializer.Deserialize<OutboxPayload>(m.Payload)!))
            .ToList();

        foreach (var (message, payload) in payloads)
            try
            {
                var primary = new UserNotification(
                    payload.PrimaryUserId,
                    payload.Message.Type,
                    payload.Message.SenderId,
                    payload.Message.TargetId
                );
                await db.UserNotifications.AddAsync(primary, ct);

                if (payload.SecondaryUserId.HasValue && payload.SecondaryUserId != payload.PrimaryUserId)
                {
                    var secondary = new UserNotification(
                        payload.SecondaryUserId.Value,
                        payload.Message.Type,
                        payload.Message.SenderId,
                        payload.Message.TargetId
                    );
                    await db.UserNotifications.AddAsync(secondary, ct);
                }

                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message {Id}", message.Id);
                message.MarkFailed(ex.Message);
            }

        var groups = payloads
            .Where(x => x.message.ProcessedAt != null)
            .GroupBy(x => x.payload.Message.Type)
            .ToList();

        foreach (var group in groups)
        {
            var userIds = group
                .SelectMany(x =>
                {
                    var ids = new List<Guid> { x.payload.PrimaryUserId };
                    if (x.payload.SecondaryUserId.HasValue)
                        ids.Add(x.payload.SecondaryUserId.Value);
                    return ids;
                })
                .Distinct()
                .ToArray();

            var notificationMessage = group.First().payload.Message;

            try
            {
                await notificationService.SendToUsersAsync(userIds, notificationMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send SignalR notification for type {Type}", group.Key);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}