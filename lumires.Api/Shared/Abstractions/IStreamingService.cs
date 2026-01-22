using lumires.Api.Shared.Models;

namespace lumires.Api.Shared.Abstractions;

public interface IStreamingService
{
    Task<List<MovieSource>> GetSourcesAsync(string tmdbId, string region = "US");
}