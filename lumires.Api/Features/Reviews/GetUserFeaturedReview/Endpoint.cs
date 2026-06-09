using FastEndpoints;
using JetBrains.Annotations;
using lumires.Api.Features.Reviews.Common;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Reviews.GetUserFeaturedReview;

[UsedImplicitly]
internal sealed record Query(Guid UserId);

[UsedImplicitly]
internal sealed record ProfileFeaturedReview(
    Guid Id,
    int FilmId,
    string FilmTitle,
    string FilmSlug,
    string? PosterPath,
    int? ReleaseYear,
    string[] Genres,
    int Runtime,
    Guid DirectorId,
    string DirectorName,
    string? Title,
    string Text,
    Guid UserId,
    string Username,
    DateTime CreatedAt,
    float? Rating,
    int LikesCount,
    int RepliesCount,
    bool IsLikedByMe,
    bool IsEditorPick,
    int MinutesRead,
    bool IsMyList) : FeaturedReviewResponse(Id, FilmId, FilmTitle, FilmSlug, PosterPath, ReleaseYear, Genres, Runtime,
    DirectorId, DirectorName, Title, Text, UserId, Username, CreatedAt, Rating, LikesCount, RepliesCount, IsLikedByMe,
    IsEditorPick, MinutesRead);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService) : Endpoint<Query, ProfileFeaturedReview>
{
    public override void Configure()
    {
        Get("/users/{username}/featured-review");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var currentUserId = currentUserService.UserId;

        var response = await db.GetFeaturedReview(query.UserId, lang, currentUserId, ct);
        
        if (response is null)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);

    }
}