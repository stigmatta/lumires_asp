using lumires.Core.Models;
using lumires.Domain.Entities;

namespace Tests.ApiTests;

internal static class Helpers
{
    internal static ExternalGenres CreateExternalGenres()
    {
        return new ExternalGenres(
        [
            new ExternalGenreItem(28, "Action"),
            new ExternalGenreItem(18, "Drama")
        ]);
    }
    
    internal static List<Genre> CreateGenres()
    {
        var genre = new Genre(28);
        genre.AddLocalization("Action", "en-US");
        genre.AddLocalization("Бойовик", "uk-UA");
        return [genre];
    }

    internal static ExternalMovie CreateExternalMovie(
        int id,
        string title,
        string poster,
        float voteAverage,
        int voteCount,
        float popularity,
        DateOnly releaseDate,
        ExternalGenres? genres = null)
    {
        return new ExternalMovie(
            id,
            title,
            null,
            poster,
            voteAverage,
            voteCount,
            popularity,
            null,
            releaseDate,
            null,
            genres ?? CreateExternalGenres()
        );
    }

}