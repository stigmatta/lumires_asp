namespace lumires.Api.Shared.Options;

public class TmdbConfig
{
    public const string Section = "TMDB";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string BearerToken { get; set; } = string.Empty;
    public string ImageBaseUrl { get; set; } = string.Empty;
}