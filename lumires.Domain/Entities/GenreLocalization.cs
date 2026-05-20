using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class GenreLocalization
{
    private GenreLocalization()
    {
    }

    public GenreLocalization(string languageCode, string name)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new DomainException("LanguageCode is required", nameof(languageCode));

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required", nameof(name));

        Id = Guid.CreateVersion7();
        LanguageCode = languageCode.Trim();
        Name = name;
    }

    public Guid Id { get; }
    public string LanguageCode { get; } = null!;
    public string Name { get; private set; } = null!;

    public Guid GenreId { get; private set; }
    public Genre Genre { get; private set; } = null!;

    internal void SetGenre(Genre genre)
    {
        Genre = genre ??
                throw new DomainException("Genre is required to be linked", nameof(genre));
        GenreId = genre.Id;
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required", nameof(name));

        Name = name;
    }
}