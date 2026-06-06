using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Users.GetUsersSummary;

[UsedImplicitly]
internal sealed record Response(int TotalMembers, int OnlineNow);

internal sealed class Endpoint(
    DataAccess db)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/users/summary");
        Description(x => x.WithTags("Users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await db.GetUsersSummary(ct);
        await Send.OkAsync(result, ct);
    }
}