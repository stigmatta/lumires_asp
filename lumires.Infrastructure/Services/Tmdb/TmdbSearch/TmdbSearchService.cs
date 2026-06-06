using Ardalis.Result;
using lumires.Core.Abstractions.Services;
using lumires.Core.Models;

namespace Infrastructure.Services.Tmdb.TmdbSearch;

public sealed class TmdbSearchService(ITmdbApi tmdbApi) : ISearchService
{
    public async Task<Result<SearchResults>> SearchAllAsync(
        string lang,
        string searchTerm,
        int page,
        CancellationToken ct)
    {
        var response = await tmdbApi.SearchMultiAsync(searchTerm, lang, page, ct);

        if (response.Content is null)
            return Result.Error("Empty response from TMDB");

        var mapped = TmdbSearchMapper.ToSearchResults(response.Content);

        return Result.Success(mapped);
    }

    public async Task<Result<IReadOnlyList<ExternalFilmShort>>> SearchFilmsAsync(
        string lang,
        string searchTerm,
        int page,
        CancellationToken ct)
    {
        var response = await tmdbApi.SearchMoviesAsync(searchTerm, lang, page, ct);

        if (response.Content is null)
            return Result.Error("Empty response from TMDB");

        var films = response.Content.Results
            .Select(TmdbSearchMapper.ToFilmShort)
            .ToList();

        return Result.Success<IReadOnlyList<ExternalFilmShort>>(films);
    }

    public async Task<Result<IReadOnlyList<ExternalPersonShort>>> SearchDirectorsAsync(
        string lang, string searchTerm, int page, CancellationToken ct)
    {
        return await SearchPersonByDepartmentAsync(lang, searchTerm, page, "Directing", ct);
    }

    public async Task<Result<IReadOnlyList<ExternalPersonShort>>> SearchActorsAsync(
        string lang, string searchTerm, int page, CancellationToken ct)
    {
        return await SearchPersonByDepartmentAsync(lang, searchTerm, page, "Acting", ct);
    }

    private async Task<Result<IReadOnlyList<ExternalPersonShort>>> SearchPersonByDepartmentAsync(
        string lang,
        string searchTerm,
        int page,
        string department,
        CancellationToken ct)
    {
        var response = await tmdbApi.SearchPersonAsync(searchTerm, lang, page, ct);

        if (response.Content is null)
            return Result.Error("Empty response from TMDB");

        var people = response.Content.Results
            .Where(p => p.KnownForDepartment == department)
            .Select(TmdbSearchMapper.ToPersonShortFromSearchItem)
            .ToList();

        return Result.Success<IReadOnlyList<ExternalPersonShort>>(people);
    }
}