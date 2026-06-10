namespace Infrastructure.Options;

public class TmdbConfig
{
    public const string Section = "TMDB";

    public required Uri BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public required string BearerToken { get; init; }
    public required string ImageBaseUrl { get; init; }

    /// <summary>
    ///     Public website base URL (not the API). Used to scrape awards data,
    ///     which TMDB does not expose through the API.
    /// </summary>
    public Uri SiteUrl { get; init; } = new("https://www.themoviedb.org");
}