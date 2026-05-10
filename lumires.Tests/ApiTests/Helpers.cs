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


    internal static List<Review> CreateReviews(int count = 5)
    {
        var movieId = Guid.NewGuid();

        var list = new List<Review>();

        for (var i = 0; i < count; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");

            var review = new Review(user.Id, movieId, $"Title {i}", $"Text {i}", i % 2 == 0 ? 5m : 3.5m, true);

            typeof(Review)
                .GetProperty(nameof(Review.Reviewer))!
                .SetValue(review, user);

            list.Add(review);
        }

        return list;
    }
}