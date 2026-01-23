using lumires.Api.Core.Models;

namespace lumires.Api.Core.Abstractions;

internal interface IStreamingService
{
    Task<List<MovieSource>> GetSourcesAsync(string tmdbId, string region = "US");
}