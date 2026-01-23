namespace lumires.Api.Core.Options;

internal class TmdbConfig
{
    public const string Section = "TMDB";

    public Uri BaseUrl { get; init; } = null!;
    public string ApiKey { get; init; } = string.Empty;
    public string BearerToken { get; init; } = string.Empty;
    public string ImageBaseUrl { get; init; } = string.Empty;
}