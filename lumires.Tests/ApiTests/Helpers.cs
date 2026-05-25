using System.Reflection;
using lumires.Core.Constants;
using lumires.Core.Models;
using lumires.Domain.Entities;

namespace Tests.ApiTests;

internal static class Helpers
{
    internal const string DefLang = LocalizationConstants.DefaultCulture;

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

    internal static List<Review> CreateReviews(int count = 5, int externalMovieId = 1)
    {
        var movieId = Guid.NewGuid();
        var movie = new Film(externalMovieId, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 4.0f, 100, 50f,
            200, "HBO");

        var list = new List<Review>();

        for (var i = 0; i < count; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");
            var review = new Review(user.Id, movieId, $"Title {i}", $"Text {i}", i % 2 == 0 ? 5f : 3.5f, true);

            typeof(Review)
                .GetProperty(nameof(Review.Reviewer))!
                .SetValue(review, user);

            typeof(Review)
                .GetProperty(nameof(Review.Film))!
                .SetValue(review, movie);

            list.Add(review);
        }

        return list;
    }

    internal static List<Review> CreateReviewsWithComments(
        int reviewsCount = 3,
        int commentsPerReview = 3)
    {
        var movieId = Guid.NewGuid();
        var movie = new Film(1, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 4.0f, 100, 50f, 200, "HBO");

        var reviews = new List<Review>();

        for (var i = 0; i < reviewsCount; i++)
        {
            var reviewer = new User(Guid.NewGuid(), $"reviewer{i}", $"reviewer{i}@mail.com");

            var review = new Review(
                reviewer.Id,
                movieId,
                $"Title {i}",
                $"Text {i}",
                5,
                true);

            typeof(Review)
                .GetProperty(nameof(Review.Reviewer))!
                .SetValue(review, reviewer);

            typeof(Review)
                .GetProperty(nameof(Review.Film))!
                .SetValue(review, movie);

            var comments = new List<ReviewComment>();

            for (var j = 0; j < commentsPerReview; j++)
            {
                var commentator = new User(Guid.NewGuid(), $"commentator{i}_{j}", $"c{i}{j}@mail.com");

                var comment = new ReviewComment(
                    Guid.NewGuid(),
                    review.Id,
                    $"Comment {j}",
                    commentator.Id
                );

                typeof(ReviewComment)
                    .GetProperty(nameof(ReviewComment.Commentator))!
                    .SetValue(comment, commentator);

                if (j % 2 == 0)
                {
                    typeof(ReviewComment)
                        .GetProperty(nameof(ReviewComment.TargetedUser))!
                        .SetValue(comment, reviewer);

                    typeof(ReviewComment)
                        .GetProperty(nameof(ReviewComment.TargetedUserId))!
                        .SetValue(comment, reviewer.Id);
                }

                comments.Add(comment);
            }

            var field = typeof(Review)
                .GetField("_reviewComments", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field is not null)
                field.SetValue(review, comments);
            else
                throw new Exception("Backing field '_reviewComments' not found");

            reviews.Add(review);
        }

        return reviews;
    }


    public static List<Film> CreateFilmsWithGenres(IEnumerable<string> genreNames)
    {
        var film = CreateFilm(DefLang, 1, "Film 1", new DateOnly(2022, 1, 1), 4.0f, 30f);

        foreach (var name in genreNames)
        {
            var genre = new Genre(Random.Shared.Next(1, 9999));
            genre.AddLocalization(name, DefLang);
            film.AddGenres([genre]);
        }

        return [film];
    }
    
    public static List<Film> CreateFilmsWithWeeklyReviews(IEnumerable<(int externalId, int reviewCount)> values)
    {
        return values
            .Select(v => CreateFilmWithWeeklyReviews(v.externalId, $"film-{v.externalId}", v.reviewCount))
            .ToList();
    }

    public static Film CreateFilmWithWeeklyReviews(int externalId, string slug, int reviewCount, bool thisWeek = true)
    {
        var film = CreateFilm(DefLang, externalId, $"Film {externalId}", new DateOnly(2022, 1, 1), 4.0f, 50f);

        typeof(Film)
            .GetProperty(nameof(Film.Slug))!
            .SetValue(film, slug);

        var reviews = new List<Review>();

        for (var i = 0; i < reviewCount; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");

            var review = new Review(
                user.Id,
                film.Id,
                $"Review title {i}",
                $"Review text {i}",
                4f,
                false);

            reviews.Add(review);
            review.SetReviewer(user);
            review.SetFilm(film);
        }

        var field = typeof(Film)
            .GetField("_reviews", BindingFlags.NonPublic | BindingFlags.Instance);

        if (field is not null)
            field.SetValue(film, reviews);
        else
            throw new Exception("Backing field '_reviews' not found");

        return film;
    }

    public static List<Film> CreateFilmsWithPopularity(IEnumerable<float> popularityValues)
    {
        return popularityValues
            .Select((p, i) => CreateFilm(DefLang, i + 1, $"Film {i}", new DateOnly(2022, 1, 1), 4.0f, p))
            .ToList();
    }


    public static List<Film> CreateFilmsWithVoteAverage(IEnumerable<float> ratings)
    {
        return ratings
            .Select((r, i) => CreateFilm(DefLang, i + 1, $"Film {i}", new DateOnly(2022, 1, 1), r, 30f))
            .ToList();
    }

    public static List<Film> CreateFilmsWithReleaseDates(IEnumerable<DateOnly> dates)
    {
        return dates
            .Select((d, i) => CreateFilm(DefLang, i + 1, $"Film {i}", d, 4.0f, 30f))
            .ToList();
    }

    private static Film CreateFilm(string lang, int externalId, string title, DateOnly releaseDate, float voteAverage,
        float popularity)
    {
        var film = new Film(externalId, releaseDate, "/poster.jpg", voteAverage, 100, popularity, 120, "Studio");
        film.AddLocalization(new FilmLocalization(DefLang, title, "Overview", "Tagline"));
        return film;
    }
}