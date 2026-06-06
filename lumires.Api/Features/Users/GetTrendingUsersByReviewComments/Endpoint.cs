using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetTrendingUsersByReviewComments;

[UsedImplicitly]
internal sealed record MemberItem(
    Guid Id,
    string Username,
    Guid ReviewId,
    string? ReviewTitle,
    int ReviewCommentsCount);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<MemberItem> Members);

internal sealed class Endpoint(
    DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Post("/users/trending");
        Description(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await db.GetTrendingUsers(ct);
        await Send.OkAsync(result, ct);
    }
}