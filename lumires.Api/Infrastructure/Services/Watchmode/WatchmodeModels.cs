using System.Text.Json.Serialization;

namespace lumires.Api.Infrastructure.Services.Watchmode;

internal class WatchmodeSearchResponse
{
    [JsonPropertyName("title_results")]
    public List<WatchmodeTitleResult> TitleResults { get; set; } = [];
}

internal class WatchmodeTitleResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tmdb_id")]
    public int TmdbId { get; set; }

    [JsonPropertyName("tmdb_type")]
    public string TmdbType { get; set; } = string.Empty; 
}

internal class WatchmodeSourceResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty; 

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; 

    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; } = string.Empty; 

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty; 

    [JsonPropertyName("price")]
    public double? Price { get; set; } 
}