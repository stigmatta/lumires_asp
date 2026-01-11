using FastEndpoints;
using lumires.Api.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;

namespace lumires.Api;

[Authorize]
public class PingEndpoint(ICurrentUserService currentUser) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/ping");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new
        {
            currentUser.UserId,
            currentUser.Email,
            currentUser.IsAuthenticated,
            currentUser.IsEmailConfirmed
        }, ct);
    }
}