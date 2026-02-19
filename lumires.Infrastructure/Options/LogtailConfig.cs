namespace Infrastructure.Options;

internal class LogtailConfig
{
    public const string SectionName = "Logtail";

    public string ApiKey { get; init; } = string.Empty;
    public required Uri BaseUrl { get; init; }
}