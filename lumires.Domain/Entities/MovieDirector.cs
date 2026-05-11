namespace lumires.Domain.Entities;

public class MovieDirector
{
    private MovieDirector()
    {
    }

    public MovieDirector(Guid personId)
    {
        Id = Guid.CreateVersion7();
        PersonId = personId;
    }

    public Guid Id { get; private set; }

    public Guid MovieId { get; private set; }
    public Movie Movie { get; private set; } = null!;

    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;
}