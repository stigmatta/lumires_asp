namespace lumires.Api.Shared.Options;

public class WatchmodeOptions
{
    public const string SectionName = "Watchmode";
    
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.watchmode.com/v1/";
    
}