using lumires.Api.Shared.Abstractions;
using lumires.Api.Shared.Models;
using lumires.Api.Shared.Options;
using Microsoft.Extensions.Options;

namespace lumires.Api.Infrastructure.Services.Watchmode;

public class WatchmodeService(HttpClient httpClient, IOptions<WatchmodeOptions> options) : IStreamingService
{
    private readonly string _apiKey = options.Value.ApiKey;
    //TODO fix
    public async Task<List<MovieSource>> GetSourcesAsync(string tmdbId, string region = "US")
    {
        try
        {
            var searchPath = $"search/?apiKey={_apiKey}&search_field=tmdb_movie_id&search_value={tmdbId}";        
            var searchResponse = await httpClient.GetFromJsonAsync<WatchmodeSearchResponse>(searchPath);

            if (searchResponse?.TitleResults == null || searchResponse.TitleResults.Count == 0)
            {
                return [];
            }

            var watchmodeId = searchResponse.TitleResults.First().Id;

            var sourcesPath = $"title/{watchmodeId}/sources/?apiKey={_apiKey}&regions={region}";
            var externalSources = await httpClient.GetFromJsonAsync<List<WatchmodeSourceResponse>>(sourcesPath);

            if (externalSources == null) return [];

            return externalSources.Select(dto => new MovieSource(
                dto.Name,           
                dto.Type, 
                dto.WebUrl,          
                dto.Format,         
                dto.Price           
            )).ToList();
        }
        catch (Exception ex)
        {
            return [];
        }
    }

}