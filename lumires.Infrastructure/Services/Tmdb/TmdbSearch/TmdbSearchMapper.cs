using Infrastructure.Services.Tmdb.Models;
using Infrastructure.Services.Tmdb.TmdbFilms;
using lumires.Core.Models;

namespace Infrastructure.Services.Tmdb.TmdbSearch;

internal static class TmdbSearchMapper
{
    public static SearchResults ToSearchResults(TmdbMultiSearchResponse response)
    {
        return new SearchResults(
            response.Results
                .Where(r => r.MediaType == "movie")
                .Select(ToFilmShort)
                .ToList(),
            response.Results
                .Where(r => r is { MediaType: "person", KnownForDepartment: "Directing" })
                .Select(ToPersonShort)
                .ToList(),
            response.Results
                .Where(r => r is { MediaType: "person", KnownForDepartment: "Acting" })
                .Select(ToPersonShort)
                .ToList(),
            response.Page,
            response.TotalPages);
    }

    public static ExternalFilmShort ToFilmShort(TmdbMultiSearchItem r)
    {
        return new ExternalFilmShort(
            r.Id,
            r.Title ?? r.Name ?? string.Empty,
            r.PosterPath,
            ParseYear(r.ReleaseDate),
            r.VoteAverage,
            r.VoteCount,
            r.Popularity,
            r.GenreIds);
    }

    public static ExternalFilmShort ToFilmShort(TmdbMovieShortResponse r)
    {
        return new ExternalFilmShort(
            r.Id,
            r.Title,
            r.PosterPath,
            ParseYear(r.ReleaseDate.ToString()),
            r.VoteAverage,
            r.VoteCount,
            r.Popularity,
            r.GenreIds);
    }

    public static ExternalPersonShort ToPersonShort(TmdbMultiSearchItem r)
    {
        return new ExternalPersonShort(
            r.Id,
            r.Name ?? string.Empty,
            r.ProfilePath,
            r.KnownForDepartment,
            r.Popularity,
            r.KnownFor.Select(ToFilmShort).ToList());
    }

    public static ExternalPersonShort ToPersonShortFromSearchItem(TmdbPersonSearchItem r)
    {
        return new ExternalPersonShort(
            r.Id,
            r.Name,
            r.ProfilePath,
            r.KnownForDepartment,
            r.Popularity,
            r.KnownFor.Select(TmdbFilmMapper.ToShort).ToList());
    }

    private static int? ParseYear(string? date)
    {
        return date is { Length: >= 4 } && int.TryParse(date[..4], out var y) ? y : null;
    }
}