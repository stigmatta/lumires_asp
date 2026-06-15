using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.Films.Contracts;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Settings.GetUserSettings;

[UsedImplicitly]
internal sealed record ProfileSettingsResponse(
    string? AvatarUrl,
    string? DisplayName,
    string? Username,
    string? Tagline);

[UsedImplicitly]
internal sealed record FavouriteFilmItem(
    int Id,
    string Title,
    string? PosterPath,
    int? ReleaseYear,
    string[] Genres,
    float VoteAverage,
    int Order) : CommonFilmListResponse(Id, Title, PosterPath, ReleaseYear, Genres, VoteAverage);

[UsedImplicitly]
internal sealed record FavouriteFilmsResponse(List<FavouriteFilmItem> FavouriteFilms);

[UsedImplicitly]
internal sealed record AccountSettings(string? EmailAddress);

[UsedImplicitly]
internal sealed record PrivacySettings(ProfileVisibility ProfileVisibility, bool IsAnyoneCanFollow, bool IsWatchlistPublic, bool AreLikesPublic, bool AreRatingsShowInFeeds);

[UsedImplicitly]
internal sealed record NotificationPreferencesResponse(
    bool NewFollower,
    bool LikesOnContent,
    bool ActivityFromFollowed,
    bool Replies,
    bool SavesOnLists,
    bool WeeklyDigest
);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    ProfileSettingsResponse ProfileSettings,
    FavouriteFilmsResponse FavouriteFilms,
    AccountSettings AccountSettings,
    PrivacySettings PrivacySettings,
    NotificationPreferencesResponse NotificationPreferences);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess db) : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/settings");
        Description(x => x.WithTags("Settings"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;

        var result = await db.GetSettings( currentUserId, lang, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}