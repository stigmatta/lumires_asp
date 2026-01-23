namespace lumires.Api.Core.Options;

internal class WatchmodeOptions
{
    public const string SectionName = "Watchmode";

    public string ApiKey { get; init; } = string.Empty;
    public Uri BaseUrl { get; init; } = new ("https://api.watchmode.com/v1/");
}