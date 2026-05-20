using lumires.Domain.Enums;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class PersonDetail
{
    private PersonDetail()
    {
    }

    public PersonDetail(
        Guid personId,
        string languageCode,
        string? biography,
        DateOnly? birthday,
        DateOnly? deathday,
        GenderType gender,
        string? placeOfBirth = null,
        string? profilePath = null)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new DomainException("LanguageCode is required", nameof(languageCode));

        Id = Guid.CreateVersion7();
        PersonId = personId;
        LanguageCode = languageCode.Trim();
        Biography = biography?.Trim();
        Birthday = birthday;
        Deathday = deathday;
        Gender = gender;
        PlaceOfBirth = placeOfBirth?.Trim();
        ProfilePath = profilePath?.Trim();
    }

    public Guid Id { get; private set; }
    public Guid PersonId { get; private set; }
    public string LanguageCode { get; private set; } = null!;
    public string? Biography { get; private set; }
    public DateOnly? Birthday { get; private set; }
    public DateOnly? Deathday { get; private set; }
    public GenderType Gender { get; private set; }
    public string? PlaceOfBirth { get; private set; }
    public string? ProfilePath { get; private set; }

    public Person Person { get; private set; } = null!;

    public void SetPerson(Person person)
    {
        Person = person ?? throw new ArgumentNullException(nameof(person));
    }
}