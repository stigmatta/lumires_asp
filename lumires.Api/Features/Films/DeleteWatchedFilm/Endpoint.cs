using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Films.DeleteWatchedFilm;

[UsedImplicitly]
internal sealed record Command(int FilmId);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/films/{filmId:int}/watched/");
        Description(x => x.WithTags("Films"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await dataAccess.DeleteWatchedFilmAsync(command, currentUserId, ct);

        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}