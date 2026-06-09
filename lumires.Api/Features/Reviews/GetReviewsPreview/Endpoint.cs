using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.GetReviewsPreview;

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<ReviewPreviewItem> Reviews);

[UsedImplicitly]
internal sealed record ReviewPreviewItem(
    Guid Id,
    int FilmId,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Text,
    int ReplyCount,
    int LikeCount,
    DateTime CreatedAt,
    ReviewCommentPreview? Comment);

[UsedImplicitly]
internal sealed record ReviewCommentPreview(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Text);

internal sealed class Endpoint(DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/reviews/preview");
        Description(x => x.WithTags("Reviews"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await db.GetReviewsPreviewAsync(ct);
        await Send.OkAsync(response, ct);
    }
}