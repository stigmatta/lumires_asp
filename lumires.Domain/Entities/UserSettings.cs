using lumires.Domain.Enums;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public class UserSettings
{

    private UserSettings()
    {
        
    }
    
    public UserSettings(Guid userId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is invalid", nameof(userId));

        UserId = userId;
        ProfileVisibility = ProfileVisibility.Everyone;
        IsAnyoneCanFollow = true;
        IsWatchlistPublic = true;
        AreLikesPublic = true;
        AreRatingsShowInFeeds = true;
        Notifications = new UserNotificationPreferences(true, true, true, true, true, true);
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public ICollection<UserFavoriteFilm> FavoriteFilms { get; private set; } = new List<UserFavoriteFilm>();
    public ProfileVisibility ProfileVisibility { get; private set; }
    public bool IsAnyoneCanFollow { get; private set; }
    public bool IsWatchlistPublic { get; private set; }
    public bool AreLikesPublic { get; private set; }
    public bool AreRatingsShowInFeeds { get; private set; }
    public UserNotificationPreferences Notifications { get; private set; } 

    public void SetFavouriteFilms(List<UserFavoriteFilm> films)
    {
        ArgumentNullException.ThrowIfNull(films);
        FavoriteFilms = films;
    }

    public void UpdateAccountSettings(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            User.SetEmail(email);
        }
    }

    public void UpdatePrivacySettings(ProfileVisibility profileVisibility, bool isAnyoneCanFollow,
        bool isWatchlistPublic, bool areLikesPublic, bool areRatingsShowInFeeds)
    {
        ProfileVisibility = profileVisibility;
        IsAnyoneCanFollow = isAnyoneCanFollow;
        IsWatchlistPublic = isWatchlistPublic;
        AreLikesPublic = areLikesPublic;
        AreRatingsShowInFeeds = areRatingsShowInFeeds;
    }
}