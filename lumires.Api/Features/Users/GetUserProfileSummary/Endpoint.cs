using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Users.GetUserProfileSummary;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record Response(
    int TotalFilmsRated,
    int ListsCreated,
    int ReviewsWritten,
    DateTimeOffset JoinedAt);

internal sealed class Endpoint(
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/users/{username}/summary");
        Description(x => x.WithTags("Users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.GetUserSummary(query.Username, ct);
        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}