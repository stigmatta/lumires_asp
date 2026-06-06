using Infrastructure.Services.Tmdb.Models;
using lumires.Core.Models;

namespace Infrastructure.Services.Tmdb.TmdbFilms;

internal static class TmdbFilmMapper
{
    public static ExternalFilm ToDomain(TmdbMovieResponse tmdb)
    {
        return new ExternalFilm(
            tmdb.Id,
            tmdb.Title,
            tmdb.Overview,
            tmdb.PosterPath,
            tmdb.VoteAverage,
            tmdb.VoteCount,
            tmdb.Popularity,
            tmdb.Runtime,
            GetProductionCompany(tmdb.ProductionCompanies),
            tmdb.BackdropPath,
            tmdb.ReleaseDate,
            GetTrailerKey(tmdb),
            tmdb.Tagline,
            ToExternalGenres(tmdb.Genres),
            GetTopExternalCast(tmdb.Credits?.Cast),
            GetExternalDirectors(tmdb.Credits?.Crew)
        );
    }

    public static ExternalFilmShort ToShort(TmdbMovieShortResponse tmdb)
    {
        return new ExternalFilmShort(
            tmdb.Id,
            tmdb.Title,
            tmdb.PosterPath,
            tmdb.ReleaseDate?.Year,
            tmdb.VoteAverage,
            tmdb.VoteCount,
            tmdb.Popularity,
            tmdb.GenreIds
        );
    }


    public static string? GetTrailerKey(TmdbMovieResponse tmdb)
    {
        return tmdb.Videos?.Results
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })
            ?.Key;
    }

    public static string GetProductionCompany(IReadOnlyCollection<TmdbProductionCompanyItem> companies)
    {
        return companies.Select(c => c.Name).FirstOrDefault() ?? string.Empty;
    }

    public static List<CastData> GetTopCastData(IReadOnlyList<CastMember>? cast)
    {
        if (cast is null || cast.Count == 0) return [];

        return cast
            .OrderBy(x => x.Order)
            .Take(6)
            .Select(x => new CastData(x.Id, x.Name, x.Character ?? string.Empty, x.Order))
            .ToList();
    }

    public static List<DirectorData> GetDirectorsData(IReadOnlyList<CrewMember>? crew)
    {
        if (crew is null || crew.Count == 0) return [];

        return crew
            .Where(x => x.Job == "Director")
            .Take(2)
            .Select(x => new DirectorData(x.Id, x.Name))
            .ToList();
    }


    private static ExternalGenres ToExternalGenres(IEnumerable<GenreResponse> genres)
    {
        return new ExternalGenres(
            genres.Select(g => new ExternalGenreItem(g.Id, g.Name)).ToList()
        );
    }

    private static IReadOnlyCollection<ExternalCastMember> GetTopExternalCast(IReadOnlyList<CastMember>? cast)
    {
        return GetTopCastData(cast)
            .Select(x => new ExternalCastMember(x.ExternalId, x.Name, x.Character, x.Order))
            .ToList();
    }

    private static IReadOnlyCollection<ExternalDirector> GetExternalDirectors(IReadOnlyList<CrewMember>? crew)
    {
        return GetDirectorsData(crew)
            .Select(x => new ExternalDirector(x.ExternalId, x.Name))
            .ToList();
    }
}

internal sealed record CastData(int ExternalId, string Name, string Character, int Order);

internal sealed record DirectorData(int ExternalId, string Name);