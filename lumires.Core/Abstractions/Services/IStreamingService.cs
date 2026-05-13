using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IStreamingService
{
    Task<Result<List<FilmSource>>> GetSourcesAsync(int tmdbId, CancellationToken ct, string region = "US");
}