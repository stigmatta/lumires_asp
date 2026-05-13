using System.Reflection;
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

    internal static List<Review> CreateReviews(int count = 5, int externalMovieId = 1)
    {
        var movieId = Guid.NewGuid();
        var movie = new Film(externalMovieId, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 8.0f, 100, 50f,
            200, "HBO");

        var list = new List<Review>();

        for (var i = 0; i < count; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");
            var review = new Review(user.Id, movieId, $"Title {i}", $"Text {i}", i % 2 == 0 ? 5m : 3.5m, true);

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
        var movie = new Film(1, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 8.0f, 100, 50f, 200, "HBO");

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
}