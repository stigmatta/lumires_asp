using lumires.Core.Constants;
using lumires.Core.Models;
using lumires.Domain.Entities;
using lumires.Domain.Enums;

namespace Tests.ApiTests;

internal static class Helpers
{
    const string DefLang = LocalizationConstants.DefaultCulture;

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

    internal static List<UserThread> CreateThreads(int count = 5)
    {
        var list = new List<UserThread>();

        for (var i = 0; i < count; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");
            var thread = new UserThread(user.Id, $"Title {i}", $"Text {i}", i % 2 == 0);

            thread.SetUser(user);

            list.Add(thread);
        }

        return list;
    }

    internal static List<Review> CreateReviews(int count = 5, int externalMovieId = 1)
    {
        var movieId = Guid.NewGuid();
        var movie = new Film(externalMovieId, DateOnly.FromDateTime(DateTime.UtcNow), "/poster.jpg", 
            4.0f, 100, 50f, 200, "HBO");

        var list = new List<Review>();

        for (var i = 0; i < count; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");
            var review = new Review(user.Id, movieId, $"Title {i}", $"Text {i}", i % 2 == 0 ? 5f : 3.5f);

            review.SetReviewer(user);
            review.SetFilm(movie);

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

            var review = new Review(reviewer.Id, movieId, $"Title {i}", $"Text {i}", 5);
            
            review.SetReviewer(reviewer);
            review.SetFilm(movie);

            var comments = new List<ReviewComment>();

            for (var j = 0; j < commentsPerReview; j++)
            {
                var commentator = new User(Guid.NewGuid(), $"commentator{i}_{j}", $"c{i}{j}@mail.com");

                var comment = new ReviewComment(Guid.NewGuid(), review.Id, $"Comment {j}", commentator.Id);
                
                comment.SetCommentator(commentator);

                if (j % 2 == 0)
                {
                    comment.SetTargetedUser(reviewer);
                }

                comments.Add(comment);
                review.AddComment(comment);
            }

            reviews.Add(review);
        }

        return reviews;
    }
    
    public static List<Film> CreateFilmsWithPopularity(IEnumerable<float> popularityValues)
    {
        return popularityValues
            .Select((p, i) => CreateFilm(i + 1, $"Film {i}", new DateOnly(2022, 1, 1), 4.0f, p))
            .ToList();
    }
    
    public static List<Film> CreateFilmsWithReleaseDates(IEnumerable<DateOnly> dates)
    {
        return dates
            .Select((d, i) => CreateFilm( i + 1, $"Film {i}", d, 4.0f, 30f))
            .ToList();
    }
    
    public static List<Film> CreateFilmsWithVoteAverage(IEnumerable<float> ratings)
    {
        return ratings
            .Select((r, i) => CreateFilm(i + 1, $"Film {i}", new DateOnly(2022, 1, 1), r, 30f))
            .ToList();
    }

    internal static List<Review> CreatePopularReviews(
        int? releaseYear,
        int count = 5,
        int daysOld = 1,
        bool spoilerFree = true)
    {
        var film = CreatePopularFilm(releaseYear.HasValue
            ? new DateOnly(releaseYear.Value, 5, 31)
            : null);
        
        var list = new List<Review>();

        for (var i = 0; i < count; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");

            var review = new Review(
                user.Id,
                film.Id,
                $"Review Title {i}",
                $"Review Text {i}",
                i % 2 == 0 ? 5f : 3.5f,
                spoilerFree);

            review.SetReviewer(user);
            review.SetFilm(film);
        
            review.SetCreatedAt(DateTime.UtcNow.AddDays(-daysOld));
        
            review.ToggleLike(user.Id);

            list.Add(review);
        }

        return list;
    }
    
    internal static List<Review> CreateTrendingReviews(
        int count = 5,
        int daysOld = 1,
        bool withTitle = true)
    {
        var film = CreatePopularFilm(new DateOnly(2020, 6, 15));
        var list = new List<Review>();
 
        for (var i = 0; i < count; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");
 
            var review = new Review(
                user.Id,
                film.Id,
                withTitle ? $"Review Title {i}" : null,
                $"Review Text {i}",
                4.5f);
 
            review.SetReviewer(user);
            review.SetFilm(film);
            review.SetCreatedAt(DateTime.UtcNow.AddDays(-daysOld));
 
            list.Add(review);
        }
 
        return list;
    }
    
    internal static List<Review> CreateTrendingReviewsWithEngagement(
        IEnumerable<(int likesCount, int commentsCount)> scores)
    {
        var film = CreatePopularFilm(new DateOnly(2020, 6, 15));
        var list = new List<Review>();
     
        foreach (var (likesCount, commentsCount) in scores)
        {
            var score = likesCount + commentsCount * 2;
            var user = new User(Guid.NewGuid(), $"user_score{score}", $"user_score{score}@gmail.com");
     
            var review = new Review(
                user.Id,
                film.Id,
                $"Title score {score}",
                $"Text score {score}",
                4f);
     
            review.SetReviewer(user);
            review.SetFilm(film);
            review.SetCreatedAt(DateTime.UtcNow.AddDays(-1));
     
            var comments = Enumerable
                .Range(0, commentsCount)
                .Select(_ => new ReviewComment(Guid.NewGuid(), review.Id, "comment", Guid.NewGuid()))
                .ToList();

            foreach (var comment in comments)
            {
                review.AddComment(comment);
            }
     
            list.Add(review);
        }
     
        return list;
    }

    private static Film CreatePopularFilm(DateOnly? releaseDate)
    {
        
        var film = new Film(
            Random.Shared.Next(1, 9999),
            releaseDate,
            "/poster.jpg",
            4.0f,
            100,
            50f,
            120,
            "Studio");

        film.AddLocalization(new FilmLocalization(DefLang, "Film Title", "Overview", "Tagline"));

        film.AddSlug($"film-slug-{film.ExternalId}");

        var person = new Person(5, PersonDepartment.Acting);
        var director = new FilmDirector(person.Id);
        film.AddDirector(director);
        director.SetPerson(person);
        
        return film;
    }

    public static List<Film> CreateFilmsWithGenres(IEnumerable<string> genreNames)
    {
        var film = CreateFilm(1, "Film 1", new DateOnly(2022, 1, 1), 4.0f, 30f);

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
        var film = CreateFilm(externalId, $"Film {externalId}", new DateOnly(2022, 1, 1), 4.0f, 50f);
        film.AddSlug(slug);

        var reviews = new List<Review>();

        for (var i = 0; i < reviewCount; i++)
        {
            var user = new User(Guid.NewGuid(), $"user{i}", $"user{i}@gmail.com");
            var review = new Review(user.Id, film.Id, $"Review title {i}", $"Review text {i}", 4f, false);

            review.SetReviewer(user);
            review.SetFilm(film);
            reviews.Add(review);
            film.AddReview(review);
        }

        return film;
    }


    private static Film CreateFilm( int externalId, string title, DateOnly releaseDate, 
        float voteAverage, float popularity)
    {
        var film = new Film(externalId, releaseDate, "/poster.jpg", voteAverage, 100, popularity, 120, "Studio");
        film.AddLocalization(new FilmLocalization(DefLang, title, "Overview", "Tagline"));
        return film;
    }
}