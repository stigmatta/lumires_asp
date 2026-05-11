using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Reviews.GetReviewsByMoviePreview;

[UsedImplicitly]
internal sealed record Query(int MovieId);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<ReviewPreviewItem> Reviews);

[UsedImplicitly]
internal sealed record ReviewPreviewItem(
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Text,
    int ReplyCount,
    int LikeCount,
    ReviewCommentPreview? Comment);

[UsedImplicitly]
internal sealed record ReviewCommentPreview(
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Text);

internal sealed class Endpoint(DataAccess db, ICurrentUserService currentUserService, IMovieResolver movieResolver)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/movies/{slug}/{movieId}/reviews/preview");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var wasExisting = await movieResolver.EnsureMovieExistsAsync(query.MovieId, lang, ct);

        var movieExists = await db.MovieExistsAsync(query.MovieId, ct);
        if (!movieExists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!wasExisting)
        {
            var newResponse = new Response([]);
            await Send.OkAsync(newResponse, ct);
            return;
        }

        var response = await db.GetReviewsPreviewAsync(query.MovieId, ct);
        await Send.OkAsync(response, ct);
    }
}