namespace lumires.Domain.Entities;

public class FilmCast
{
    private FilmCast()
    {
    }

    public FilmCast(Guid personId, string character, int order)
    {
        Id = Guid.CreateVersion7();
        PersonId = personId;
        Character = character;
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid FilmId { get; private set; }
    public Film Film { get; private set; } = null!;

    public Guid PersonId { get; private set; }
    public Person Person { get; private set; } = null!;

    public string Character { get; private set; } = string.Empty;
    public int Order { get; private set; }
}