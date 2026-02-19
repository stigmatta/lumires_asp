namespace Infrastructure.Options;

public class EmailSenderConfig
{
    public const string Section = "EmailSender";

    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
}