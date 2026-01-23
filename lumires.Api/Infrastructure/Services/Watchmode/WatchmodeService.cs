using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Models;

namespace lumires.Api.Infrastructure.Services.Watchmode;

public sealed class WatchmodeService(IWatchmodeApi watchmodeApi) : IStreamingService
{
    public async Task<List<MovieSource>> GetSourcesAsync(string tmdbId, string region = "US")
    {
        var searchResponse = await watchmodeApi.SearchByTmdbIdAsync(tmdbId);

        var firstResult = searchResponse.TitleResults.Count > 0 
            ? searchResponse.TitleResults[0] 
            : null;
        
        if (firstResult == null) return [];

        var externalSources = await watchmodeApi.GetSourcesAsync(firstResult.Id, region);

        return [.. externalSources.Select(dto => new MovieSource(
            dto.Name,
            dto.Type,
            dto.WebUrl,
            dto.Format,
            dto.Price
        ))];
    }
}