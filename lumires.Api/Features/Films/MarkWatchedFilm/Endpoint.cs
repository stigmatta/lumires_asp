using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Films.MarkWatchedFilm;


[UsedImplicitly]
internal sealed record Command(int FilmId);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess dataAccess,
    IFilmResolver filmResolver)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Post("/films/{Slug}/{filmId:int}/watched/");
        Description(x => x.WithTags("Films"));
        Throttle(5, 2);
    }
    
    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;
        
        await filmResolver.EnsureFilmExistsAsync(command.FilmId, lang, ct);

        var result = await dataAccess.MarkWatchedAsync(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
    
}