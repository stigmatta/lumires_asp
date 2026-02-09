namespace Infrastructure.Options;

internal class LogtailConfig
{
    public const string SectionName = "Logtail";

    public string ApiKey { get; init; } = string.Empty;
    public Uri BaseUrl { get; init; } = new("https://s1694678.eu-nbg-2.betterstackdata.com");
}