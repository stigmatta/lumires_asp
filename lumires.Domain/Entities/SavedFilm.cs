using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public class SavedFilm
{
    private SavedFilm()
    {
    }

    public SavedFilm(Guid userId, Guid filmId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is invalid", nameof(userId));

        if (filmId == Guid.Empty) throw new DomainException("Film id is invalid");
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
    public DateTimeOffset CreatedAt { get; }
}