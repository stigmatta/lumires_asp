using lumires.Api.Core.Models;

namespace lumires.Api.Core.Abstractions;

internal interface IExternalMovieService
{
    Task<MovieImportResult?> GetMovieDetailsAsync(int movieId, CancellationToken ct = default);
}