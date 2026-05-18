using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Auth.CreateProfile;

[UsedImplicitly]
internal sealed record Command(
    Guid Id,
    string Username,
    string Email
);

[UsedImplicitly]
internal sealed record Response(
    Guid Id,
    string Email,
    string? Username,
    string? AvatarUrl
);

internal sealed class Endpoint(ICurrentUserService currentUserService, DataAccess dataAccess)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/auth/register");
        Description(x => x.WithTags("Auth"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var userIdClaim = currentUserService.UserId;
        var emailClaim = currentUserService.Email;

        if (userIdClaim != command.Id || emailClaim != command.Email)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var isUserExists = await dataAccess.IsUserExistsAsync(command, ct);
        if (isUserExists)
        {
            await Send.ResultAsync(TypedResults.Conflict());
            return;
        }

        var createdUser = await dataAccess.CreateUserAsync(command, ct);

        await Send.CreatedAtAsync<GetMe.Endpoint>(
            responseBody: createdUser,
            cancellation: ct);
    }
}