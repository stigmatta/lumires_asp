using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Settings.UpdateAccountSettings;

[UsedImplicitly]
internal sealed record Command(string? Email);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Put("/settings/account");
        Description(x => x.WithTags("Settings"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.UpdateAccountSettings(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}