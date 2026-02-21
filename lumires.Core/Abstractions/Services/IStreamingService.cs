using Ardalis.Result;
using Core.Models;

namespace Core.Abstractions.Services;

public interface IStreamingService
{
    Task<Result<List<MovieSource>>> GetSourcesAsync(int tmdbId, CancellationToken ct, string region = "US");
}