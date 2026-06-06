using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetThisWeekMostActiveUsers;

[UsedImplicitly]
internal sealed record MemberItem(Guid Id, string Username, int WeeklyReviewsCount, int WeeklyListsCount);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<MemberItem> Members);

internal sealed class Endpoint(
    DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Post("/users/most-active");
        Description(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await db.GetMostActiveUsersAsync(ct);
        await Send.OkAsync(result, ct);
    }
}