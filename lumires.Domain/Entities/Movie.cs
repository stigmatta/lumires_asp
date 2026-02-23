using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class Movie
{
    private readonly List<MovieLocalization> _localizations = [];

    public Guid Id { get; }
    public int ExternalId { get; }
    public int Year { get; private set; }
    public string PosterPath { get; private set; } = null!;
    public string? BackdropPath { get; private set; }
    public string? TrailerUrl { get; private set; }

    public IReadOnlyCollection<MovieLocalization> Localizations => _localizations.AsReadOnly();

    private Movie() { } 

    public Movie(int externalId, int year, string posterPath, string? backdropPath = null, string? trailerUrl = null)
    {
        if (externalId <= 0)
            throw new MovieValidationException("ExternalId must be positive", nameof(externalId));

        if (year is < 1888 or > 2126)
            throw new MovieValidationException("Invalid movie year", nameof(year));

        if (string.IsNullOrWhiteSpace(posterPath))
            throw new MovieValidationException("PosterPath is required", nameof(posterPath));

        Id = Guid.CreateVersion7();
        ExternalId = externalId;
        Year = year;
        PosterPath = posterPath;
        BackdropPath = backdropPath;
        TrailerUrl = trailerUrl;
    }

    /// <summary>
    /// Adds a localization and sets the navigation & foreign key automatically.
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