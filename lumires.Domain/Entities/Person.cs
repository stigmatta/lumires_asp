using lumires.Domain.Enums;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class Person
{
    private readonly List<PersonLocalization> _localizations = [];
    private readonly List<PersonDetail> _details = [];

    private Person()
    {
    } // EF Core

    public Person(int externalId, PersonDepartment department)
    {
        if (externalId <= 0)
            throw new DomainException("ExternalId must be positive", nameof(externalId));

        Id = Guid.CreateVersion7();
        ExternalId = externalId;
        PersonDepartment = department;
    }

    public Guid Id { get; private set; }
    public int ExternalId { get; private set; }
    public PersonDepartment PersonDepartment { get; private set; }

    public IReadOnlyCollection<PersonDetail> Details => _details.AsReadOnly();

    public IReadOnlyCollection<PersonLocalization> Localizations => _localizations.AsReadOnly();

    public ICollection<FilmCast> FilmCasts { get; private set; } = new List<FilmCast>();
    public ICollection<FilmDirector> FilmDirectors { get; private set; } = new List<FilmDirector>();

    public void AddLocalization(PersonLocalization localization)
    {
        ArgumentNullException.ThrowIfNull(localization);

        if (_localizations.Any(l => l.LanguageCode == localization.LanguageCode))
            throw new DomainException($"Localization for language '{localization.LanguageCode}' already exists");

        localization.SetPerson(this);
        _localizations.Add(localization);
    }

    public void AddDetail(PersonDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        
        if (_details.Any(l => l.LanguageCode == detail.LanguageCode))
            throw new DomainException($"Localization for language '{detail.LanguageCode}' already exists");
        
        detail.SetPerson(this);
        _details.Add(detail);        
    }

    public string GetName(string languageCode)
    {
        return Localizations.FirstOrDefault(pl => pl.LanguageCode == languageCode)?.Name
               ?? Localizations.FirstOrDefault(pl => pl.LanguageCode == "en")?.Name
               ?? Localizations.FirstOrDefault()?.Name
               ?? "Unknown";
    }
}