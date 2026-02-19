namespace Infrastructure.Options;

public class TmdbConfig
{
    public const string Section = "TMDB";

    public required Uri BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public required string BearerToken { get; init; }
    public required string ImageBaseUrl { get; init; }
}