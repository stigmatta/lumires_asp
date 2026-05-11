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

    public ICollection<MovieCast> MovieCasts { get; private set; } = new List<MovieCast>();
    public ICollection<MovieDirector> MovieDirectors { get; private set; } = new List<MovieDirector>();
}