using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Models;

namespace lumires.Api.Infrastructure.Services.Tmdb;

public sealed class TmdbService(ITmdbApi tmdbApi, ICurrentUserService currentUserService) : IExternalMovieService
{
    public async Task<MovieImportResult?> GetMovieDetailsAsync(int movieId, CancellationToken ct = default)
    {
        var lang = currentUserService.LangCulture;
        var response = await tmdbApi.GetMovieAsync(movieId, lang, ct);

        if (!response.IsSuccessStatusCode) return null;

        var result = response.Content;

        if (!string.IsNullOrWhiteSpace(result?.Overview) || lang == "en-US") return result;

        var fallback = await tmdbApi.GetMovieAsync(movieId, "en-US", ct);
        return fallback.Content ?? result;
    }
}