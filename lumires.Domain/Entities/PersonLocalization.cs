using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class PersonLocalization
{
    private PersonLocalization()
    {
    }

    public PersonLocalization(string languageCode, string name)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            throw new DomainException("LanguageCode is required", nameof(languageCode));

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required", nameof(name));

        Id = Guid.CreateVersion7();
        LanguageCode = languageCode.Trim();
        Name = name.Trim();
    }

    public Guid Id { get; private set; }
    public Guid PersonId { get; private set; }

    public string LanguageCode { get; private set; }
    public string Name { get; private set; }

    public Person Person { get; private set; } = null!;

    internal void SetPerson(Person person)
    {
        Person = person ?? throw new ArgumentNullException(nameof(person));
    }
}