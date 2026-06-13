using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Films.WatchFilm;

[UsedImplicitly]
internal sealed record Command(int FilmId);

[UsedImplicitly]
internal sealed record Response(bool IsWatched);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess,
    IFilmResolver filmResolver)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/films/{filmId:int}/watch/");
        Description(x => x.WithTags("Films"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;

        await filmResolver.EnsureFilmExistsAsync(command.FilmId, lang, ct);

        var result = await dataAccess.ToggleWatchedAsync(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
