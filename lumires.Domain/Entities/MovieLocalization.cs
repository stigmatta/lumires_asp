using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class MovieLocalization
{
    private MovieLocalization()
    {
    }

    public MovieLocalization(string languageCode, string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new MovieLocalizationValidationException("LanguageCode is required", nameof(languageCode));

        if (string.IsNullOrWhiteSpace(title))
            throw new MovieLocalizationValidationException("Title is required", nameof(title));

        Id = Guid.CreateVersion7();
        LanguageCode = languageCode.Trim();
        Title = title;
        Description = description;
    }

    public Guid Id { get; }
    public string LanguageCode { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }

    public Guid MovieId { get; private set; }
    public Movie Movie { get; private set; } = null!;

    /// <summary>
    ///     Internal method to set Movie and MovieId consistently.
    ///     Called by Movie.AddLocalization.
    /// </summary>
    internal void SetMovie(Movie movie)
    {
        Movie = movie ??
                throw new MovieLocalizationValidationException("Movie is required to be linked", nameof(movie));
        MovieId = movie.Id;
    }

    public void Update(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new MovieLocalizationValidationException("Title is required", nameof(title));

        Title = title;
        Description = description;
    }
}