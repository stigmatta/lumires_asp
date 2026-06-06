using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Films.GetMostReviewedFilmByActor;

[UsedImplicitly]
internal sealed record Query(int ActorId);

[UsedImplicitly]
internal sealed record ReviewCommentItem(
    Guid Id,
    Guid UserId,
    string Username,
    string Text,
    DateTime CreatedAt,
    int LikesCount,
    bool IsLikedByMe);

[UsedImplicitly]
internal sealed record Response(
    int FilmId,
    string? FilmTitle,
    string? FilmSlug,
    string? PosterPath,
    int ReviewsCount,
    Guid UserId,
    string? Username,
    string? AvatarUrl,
    string? Title,
    string Text,
    int LikesCount,
    bool IsLikedByMe,
    IReadOnlyCollection<ReviewCommentItem> Comments
);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IPersonResolver personResolver,
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/directors/{actorId:int}/films/most-reviewed");
        Description(x => x.WithTags("Films"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var currentUserId = currentUserService.UserId;

        await personResolver.EnsurePersonExistsAsync((query.ActorId, nameof(PersonDepartment.Acting)), lang, ct);

        var result = await db.GetMostReviewedFilmByActor(query.ActorId, lang, currentUserId, ct);

        if (result is null)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}