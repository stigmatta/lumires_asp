namespace lumires.Domain.Entities;

public class Person
{
    private Person()
    {
    }

    public Person(int externalId, string name)
    {
        Id = Guid.CreateVersion7();
        ExternalId = externalId;
        Name = name.Trim();
    }

    public Guid Id { get; private set; }
    public int ExternalId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ICollection<FilmCast> FilmCasts { get; private set; } = new List<FilmCast>();
    public ICollection<FilmDirector> FilmDirectors { get; private set; } = new List<FilmDirector>();
}