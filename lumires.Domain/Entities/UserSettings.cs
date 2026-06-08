using lumires.Domain.Enums;

namespace lumires.Domain.Entities;

public class UserSettings
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public ICollection<Film> FavoriteFilms { get; private set; } = new List<Film>();
    public ProfileVisibility ProfileVisibility { get; private set; }
    public bool IsAnyoneCanFollow { get; private set; }
    public bool IsWatchlistPublic { get; private set; }
    public bool AreLikesPublic { get; private set; }
    public bool AreRatingsShowInFeeds { get; private set; }
    public bool IsLikesPublic { get; private set; }

    public UserNotificationPreferences Notifications { get; private set; } =
        new(true, true, true, true, true, true);
}