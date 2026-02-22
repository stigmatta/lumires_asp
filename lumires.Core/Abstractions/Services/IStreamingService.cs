using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IStreamingService
{
    Task<Result<List<MovieSource>>> GetSourcesAsync(int tmdbId, CancellationToken ct, string region = "US");
}