using FastEndpoints;
using JetBrains.Annotations;

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
    int Friends,
    bool IsMe,
    bool? IsFollowed,
    bool? IsBlocked);

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

        await Send.OkAsync(response, ct);
    }
}