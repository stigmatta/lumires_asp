using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Settings.GetUserSettings;

[UsedImplicitly]
internal sealed class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    public async Task<Result<Response>> GetSettings(Guid userId, string lang, CancellationToken ct)
    {
        // Project in the database (like GetUserFavouriteFilms) instead of materializing
        // the entity graph and projecting in memory. The old approach dereferenced
        // x.Film / x.Film.Localizations / x.Film.Genres, which were never Included,
        // throwing a NullReferenceException (500) for any user with favourite films.
        var data = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                SettingsId = u.UserSettings.Id,
                u.AvatarUrl,
                u.DisplayName,
                u.Username,
                u.Tagline,
                u.Email,
                u.UserSettings.ProfileVisibility,
                u.UserSettings.IsAnyoneCanFollow,
                u.UserSettings.IsWatchlistPublic,
                u.UserSettings.AreLikesPublic,
                u.UserSettings.AreRatingsShowInFeeds,
                NewFollower = u.UserSettings.Notifications.NewFollower,
                LikesOnContent = u.UserSettings.Notifications.LikesOnContent,
                ActivityFromFollowed = u.UserSettings.Notifications.ActivityFromFollowed,
                RepliesAndMentions = u.UserSettings.Notifications.RepliesAndMentions,
                SavesOnLists = u.UserSettings.Notifications.SavesOnLists,
                WeeklyDigest = u.UserSettings.Notifications.WeeklyDigest,
                FavouriteFilms = u.UserSettings.FavoriteFilms
                    .OrderBy(f => f.Order)
                    .Select(f => new
                    {
                        f.Film.ExternalId,
                        Title = f.Film.Localizations
                            .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                            .OrderByDescending(l => l.LanguageCode == lang)
                            .Select(l => l.Title)
                            .FirstOrDefault() ?? string.Empty,
                        f.Film.PosterPath,
                        ReleaseYear = f.Film.ReleaseDate.HasValue ? f.Film.ReleaseDate.Value.Year : (int?)null,
                        Genres = f.Film.Genres
                            .Select(g => g.Localizations
                                .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                                .OrderByDescending(gl => gl.LanguageCode == lang)
                                .Select(gl => gl.Name)
                                .FirstOrDefault() ?? string.Empty)
                            .ToArray(),
                        f.Film.VoteAverage,
                        f.Order
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (data is null) return Result.NotFound();

        var favouriteFilms = data.FavouriteFilms
            .Select(f => new FavouriteFilmItem(
                f.ExternalId,
                f.Title,
                f.PosterPath,
                f.ReleaseYear,
                f.Genres,
                f.VoteAverage,
                f.Order))
            .ToList();

        return new Response(data.SettingsId,
            new ProfileSettingsResponse(data.AvatarUrl, data.DisplayName, data.Username, data.Tagline),
            new FavouriteFilmsResponse(favouriteFilms),
            new AccountSettings(data.Email),
            new PrivacySettings(data.ProfileVisibility, data.IsAnyoneCanFollow, data.IsWatchlistPublic,
                data.AreLikesPublic, data.AreRatingsShowInFeeds),
            new NotificationPreferencesResponse(data.NewFollower, data.LikesOnContent,
                data.ActivityFromFollowed, data.RepliesAndMentions, data.SavesOnLists,
                data.WeeklyDigest));
    }
}