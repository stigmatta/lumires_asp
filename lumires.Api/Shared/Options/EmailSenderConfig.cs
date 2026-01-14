namespace lumires.Api.Shared.Options;

public class EmailSenderConfig
{
    public const string Section = "EmailSender";

    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}