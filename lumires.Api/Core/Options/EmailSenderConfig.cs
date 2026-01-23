namespace lumires.Api.Core.Options;

internal class EmailSenderConfig
{
    public const string Section = "EmailSender";

    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}