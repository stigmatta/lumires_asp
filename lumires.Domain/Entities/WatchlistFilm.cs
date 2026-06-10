using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public class WatchlistFilm
{
    private WatchlistFilm() { }

    public WatchlistFilm(Guid userId, Guid filmId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is invalid");
        if (filmId == Guid.Empty) throw new DomainException("FilmId is invalid");

        Id = Guid.CreateVersion7();
        UserId = userId;
        FilmId = filmId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid FilmId { get; private set; }
    public Film Film { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
}
