namespace lumires.Domain.Entities;

public class FilmDirector
{
    private FilmDirector()
    {
    }

    public FilmDirector(Guid personId)
    {
        Id = Guid.CreateVersion7();
        PersonId = personId;
    }

    public Guid Id { get; private set; }

    public Guid FilmId { get; private set; }
    public Film Film { get; private set; } = null!;

    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;
}