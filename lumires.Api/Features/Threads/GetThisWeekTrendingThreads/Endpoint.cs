using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Threads.GetThisWeekTrendingThreads;

[UsedImplicitly]
internal sealed record Response(IReadOnlyList<TrendingThreadItem> Items);

[UsedImplicitly]
internal sealed record TrendingThreadItem(
    Guid Id,
    string? Title,
    string? Image,
    Guid UserId,
    string Username,
    DateTime CreatedAt,
    int ReplyCount
);

internal sealed class Endpoint(DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/threads/trending/weekly");
        Description(x => x.WithTags("Threads"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await db.GetTrendingThreadsWeeklyAsync(ct);

        if (response is null)
        {
            await Send.OkAsync(new Response([]), ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}