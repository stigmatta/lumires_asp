using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class Movie
{
    private readonly List<MovieLocalization> _localizations = [];

    public Movie(int externalId, DateOnly releaseDate, string posterPath, float voteAverage,
        int voteCount, float popularity, string? backdropPath = null, string? trailerUrl = null)
        : this(Guid.CreateVersion7(), externalId, releaseDate, posterPath, voteAverage, voteCount, popularity,
            backdropPath, trailerUrl)
    {
    }

    public Movie(Guid id, int externalId, DateOnly releaseDate, string posterPath, float voteAverage,
        int voteCount, float popularity, string? backdropPath = null, string? trailerUrl = null)
    {
        if (externalId <= 0)
            throw new MovieValidationException("ExternalId must be positive", nameof(externalId));

        if (releaseDate < new DateOnly(1888, 1, 1) || releaseDate > new DateOnly(2126, 12, 31))
            throw new MovieValidationException("Invalid movie release date", nameof(releaseDate));

        if (voteAverage is < 0 or > 10)
            throw new MovieValidationException("Invalid average vote rating", nameof(voteAverage));

        if (voteCount < 0)
            throw new MovieValidationException("Invalid vote count", nameof(voteCount));

        if (popularity < 0)
            throw new MovieValidationException("Invalid popularity", nameof(popularity));

        Id = id;
        ExternalId = externalId;
        ReleaseDate = releaseDate;
        PosterPath = posterPath;
        VoteAverage = voteAverage;
        VoteCount = voteCount;
        Popularity = popularity;
        BackdropPath = backdropPath;
        TrailerUrl = trailerUrl;
    }

    public Guid Id { get; }
    public int ExternalId { get; }
    public DateOnly ReleaseDate { get; private set; }
    public string? PosterPath { get; private set; }
    public string? BackdropPath { get; private set; }
    public string? TrailerUrl { get; private set; }
    public float VoteAverage { get; private set; }
    public int VoteCount { get; private set; }
    public float Popularity { get; private set; }


    public IReadOnlyCollection<MovieLocalization> Localizations => _localizations.AsReadOnly();

    /// <summary>
    ///     Adds a localization and sets the navigation & foreign key automatically.
    /// </summary>
    public void AddLocalization(MovieLocalization localization)
    {
        ArgumentNullException.ThrowIfNull(localization);

        if (_localizations.Any(l => l.LanguageCode == localization.LanguageCode))
            throw new InvalidMovieOperationException("Localization already exists for this language");

        localization.SetMovie(this);
        _localizations.Add(localization);
    }
}