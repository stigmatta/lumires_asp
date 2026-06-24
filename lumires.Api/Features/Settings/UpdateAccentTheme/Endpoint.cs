using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Settings.UpdateAccentTheme;

[UsedImplicitly]
internal sealed record Command(string? AccentTheme);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Put("/settings/accent");
        Description(x => x.WithTags("Settings"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.UpdateAccentTheme(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
