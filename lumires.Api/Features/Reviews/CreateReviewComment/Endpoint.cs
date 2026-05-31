using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Reviews.CreateReviewComment;

[UsedImplicitly]
internal sealed record Command(Guid ReviewId, string Text, Guid? TargetedUserId, bool IsSpoilerFree);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Text,
    DateOnly CreatedAt,
    bool IsSpoilerFree
);

internal sealed class Endpoint(
    DataAccess db)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/films/{Slug}/{filmId}/reviews/{ReviewId}/reply");
        Description(x => x.WithTags("Reviews"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var result = await db.CreateReviewCommentAsync(command, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.CreatedAtAsync<GetReview.Endpoint>(
            responseBody: result.Value,
            cancellation: ct);
    }
}