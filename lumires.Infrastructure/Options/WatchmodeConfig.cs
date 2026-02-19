namespace Infrastructure.Options;

internal class WatchmodeOptions
{
    public const string SectionName = "Watchmode";

    public required string ApiKey { get; init; }
    public required Uri BaseUrl { get; init; }
}