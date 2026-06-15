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
        var user = await db.Users
            .Include(u => u.UserSettings)
            .ThenInclude(s => s.FavoriteFilms)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return Result.NotFound();

        if (user.Id != userId) return Result.Forbidden();

        var settings = user.UserSettings;
        var notifications = settings.Notifications;

        return new Response(settings.Id,
            new ProfileSettingsResponse(user.AvatarUrl, user.DisplayName, user.Username,
                user.Tagline),
            new FavouriteFilmsResponse(
                [
                    .. settings.FavoriteFilms
                        .Select(x => new FavouriteFilmItem(
                            x.Film.ExternalId,
                            x.Film.Localizations
                                .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                                .OrderByDescending(l => l.LanguageCode == lang)
                                .Select(l => l.Title)
                                .FirstOrDefault() ?? string.Empty,
                            x.Film.PosterPath,
                            x.Film.ReleaseDate?.Year,
                            [
                                .. x.Film.Genres
                                    .Select(g =>
                                        g.Localizations
                                            .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                                            .OrderByDescending(gl => gl.LanguageCode == lang)
                                            .Select(gl => gl.Name)
                                            .FirstOrDefault()!)
                            ],
                            x.Film.VoteAverage,
                            x.Order
                        ))
                ]
            ),
            new AccountSettings(user.Email),
            new PrivacySettings(settings.ProfileVisibility, settings.IsAnyoneCanFollow, settings.IsWatchlistPublic,
                settings.AreLikesPublic, settings.AreRatingsShowInFeeds),
            new NotificationPreferencesResponse(notifications.NewFollower, notifications.LikesOnContent,
                notifications.ActivityFromFollowed, notifications.RepliesAndMentions, notifications.SavesOnLists,
                notifications.WeeklyDigest));
    }
}