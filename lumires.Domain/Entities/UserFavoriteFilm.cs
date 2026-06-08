using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public class UserFavoriteFilm
{
    private UserFavoriteFilm() { }

    public UserFavoriteFilm(Guid userSettingsId, Guid filmId, int order)
    {
        if (userSettingsId == Guid.Empty) throw new DomainException("UserSettingsId is invalid");
        if (filmId == Guid.Empty) throw new DomainException("FilmId is invalid");

        Id = Guid.CreateVersion7();
        UserSettingsId = userSettingsId;
        FilmId = filmId;
        Order = order;
    }

    public Guid Id { get; private set; }
    public Guid UserSettingsId { get; private set; }
    public UserSettings UserSettings { get; private set; } = null!;

    public Guid FilmId { get; private set; }
    public Film Film { get; private set; } = null!;

    public int Order { get; private set; }
}