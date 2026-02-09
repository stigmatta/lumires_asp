using Contracts.Models;

namespace Contracts.Abstractions;

public interface IStreamingService
{
    Task<List<MovieSource>> GetSourcesAsync(int tmdbId, CancellationToken ct, string region = "US");
}