using Infrastructure.Services.Tmdb.Models;
using Infrastructure.Services.Tmdb.TmdbFilms;
using lumires.Core.Models;

namespace Infrastructure.Services.Tmdb.TmdbSearch;

internal static class TmdbSearchMapper
{
    public static SearchResults ToSearchResults(TmdbMultiSearchResponse response) => new(
        Films: response.Results
            .Where(r => r.MediaType == "movie")
            .Select(ToFilmShort)
            .ToList(),
        Directors: response.Results
            .Where(r => r is { MediaType: "person", KnownForDepartment: "Directing" })
            .Select(ToPersonShort)
            .ToList(),
        Actors: response.Results
            .Where(r => r is { MediaType: "person", KnownForDepartment: "Acting" })
            .Select(ToPersonShort)
            .ToList(),
        Page: response.Page,
        TotalPages: response.TotalPages);

    public static ExternalFilmShort ToFilmShort(TmdbMultiSearchItem r) => new(
        ExternalId: r.Id,
        Title: r.Title ?? r.Name ?? string.Empty,
        PosterPath: r.PosterPath,
        ReleaseYear: ParseYear(r.ReleaseDate),
        VoteAverage: r.VoteAverage,
        VoteCount: r.VoteCount,
        Popularity: r.Popularity,
        GenreIds: r.GenreIds);

    public static ExternalFilmShort ToFilmShort(TmdbMovieShortResponse r) => new(
        ExternalId: r.Id,
        Title: r.Title,
        PosterPath: r.PosterPath,
        ReleaseYear: ParseYear(r.ReleaseDate.ToString()),
        VoteAverage: r.VoteAverage,
        VoteCount: r.VoteCount,
        Popularity: r.Popularity,
        GenreIds: r.GenreIds);

    public static ExternalPersonShort ToPersonShort(TmdbMultiSearchItem r) => new(
        ExternalId: r.Id,
        Name: r.Name ?? string.Empty,
        ProfilePath: r.ProfilePath,
        KnownForDepartment: r.KnownForDepartment,
        Popularity: r.Popularity,
        KnownFor: r.KnownFor.Select(ToFilmShort).ToList());

    public static ExternalPersonShort ToPersonShortFromSearchItem(TmdbPersonSearchItem r) => new(
        ExternalId: r.Id,
        Name: r.Name,
        ProfilePath: r.ProfilePath,
        KnownForDepartment: r.KnownForDepartment,
        Popularity: r.Popularity,
        KnownFor: r.KnownFor.Select(TmdbFilmMapper.ToShort).ToList());

    private static int? ParseYear(string? date) =>
        date is { Length: >= 4 } && int.TryParse(date[..4], out var y) ? y : null;
}