using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Auth.GetMe;

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Email,
    string? Username,
    string? AvatarUrl
);

internal sealed class Endpoint(ICurrentUserService currentUserService, DataAccess dataAccess)
    : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/auth/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = currentUserService.UserId;

        var user = await dataAccess.UserOrNullAsync(userId, ct);
        if (user is null)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        await Send.OkAsync(user, ct);
    }
}