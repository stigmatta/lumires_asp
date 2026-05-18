using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class Genre
{
    private readonly List<GenreLocalization> _localizations = [];

    private Genre()
    {
    }

    public Genre(int externalId)
    {
        if (externalId <= 0)
            throw new DomainException("ExternalId must be positive", nameof(externalId));

        Id = Guid.CreateVersion7();
        ExternalId = externalId;
    }

    public Guid Id { get; }
    public int ExternalId { get; }
    public IReadOnlyCollection<GenreLocalization> Localizations => _localizations.AsReadOnly();

    public void AddLocalization(string name, string languageCode)
    {
        if (_localizations.Any(l => l.LanguageCode == languageCode))
            throw new DomainException($"Localization for '{languageCode}' already exists",
                nameof(languageCode));

        var localization = new GenreLocalization(languageCode, name);
        localization.SetGenre(this);
        _localizations.Add(localization);
    }

    public void UpdateOrAddLocalization(string name, string languageCode)
    {
        var existing = _localizations.FirstOrDefault(l => l.LanguageCode == languageCode);

        if (existing is null)
            AddLocalization(name, languageCode);
        else
            existing.Update(name);
    }
}