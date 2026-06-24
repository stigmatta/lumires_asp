using FastEndpoints;
using JetBrains.Annotations;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Users.GetUserProfile;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record Relationship(UserRelationshipType Type, UserRelationshipStatus Status);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Username,
    string? DisplayName,
    string Pronouns,
    string? Location,
    string? Tagline,
    string? AvatarUrl,
    string? Biography,
    string? AccentTheme,
    int Followers,
    int Followings,
    int Friends,
    bool IsMe,
    Relationship? IncomingRelationship,
    Relationship? OutgoingRelationship,
    int ReviewsWritten,
    int ThreadsWritten,
    int ListsCreated);

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

        if (!response.IsSuccess)
        {
            await HttpContext.SendErrorAsync(response.Status, ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}