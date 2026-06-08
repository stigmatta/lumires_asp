using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Users.GetUserProfile;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record Response(
    string Username,
    string? DisplayName,
    string Pronouns,
    string? Location,
    string? Tagline,
    string? Biography,
    int Followers,
    int Followings,
    bool IsMe);

internal sealed class Endpoint(
    DataAccess db)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/users/{username}");
        Description(x => x.WithTags("Users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var response = await db.GetUserProfile(query.Username, ct);
        if (response is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}