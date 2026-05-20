using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class FilmLocalization
{
    private FilmLocalization()
    {
    }

    public FilmLocalization(string languageCode, string title, string? description, string? tagline)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new DomainException("LanguageCode is required", nameof(languageCode));

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required", nameof(title));

        Id = Guid.CreateVersion7();
        LanguageCode = languageCode.Trim();
        Title = title;
        Description = description;
        Tagline = tagline;
    }

    public Guid Id { get; }
    public string LanguageCode { get; } = null!;
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Tagline { get; private set; }

    public Guid FilmId { get; private set; }
    public Film Film { get; private set; } = null!;

    /// <summary>
    ///     Internal method to set Movie and MovieId consistently.
    ///     Called by Movie.AddLocalization.
    /// </summary>
    internal void SetFilm(Film film)
    {
        Film = film ??
               throw new DomainException("Movie is required to be linked", nameof(film));
        FilmId = film.Id;
    }

    public void Update(string title, string? description, string? tagline)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required", nameof(title));

        Title = title;
        Description = description;
        Tagline = tagline;
    }
}