namespace lumires.Domain.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public OutboxMessage(string type, string payload)
    {
        Id = Guid.CreateVersion7();
        Type = type;
        Payload = payload;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public string Type { get; } = null!;
    public string Payload { get; } = null!;
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    public void MarkProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}