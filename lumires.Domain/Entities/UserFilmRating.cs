using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class UserFilmRating
{
    private UserFilmRating()
    {
    }

    public UserFilmRating(Guid userId, Guid filmId, float rating)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required", nameof(UserId));

        if (filmId == Guid.Empty)
            throw new DomainException("FilmId is required", nameof(UserId));

        if (rating is < 0 or > 5)
            throw new DomainException("Rating is not correct", nameof(Rating));

        Id = Guid.CreateVersion7();
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
        FilmId = filmId;
        Rating = rating;
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public User User { get; } = null!;
    public Guid FilmId { get; }
    public Film Film { get; } = null!;
    public float Rating { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }


    public void UpdateRating(float rating)
    {
        if (rating is < 0 or > 5)
            throw new DomainException("Rating is not correct", nameof(UserId));
        Rating = rating;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}