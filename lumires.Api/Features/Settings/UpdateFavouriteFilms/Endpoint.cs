using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Settings.UpdateFavouriteFilms;

[UsedImplicitly]
internal sealed record FavouriteFilm(int ExternalId, int Order);

[UsedImplicitly]
internal sealed record Command(List<FavouriteFilm> FavouriteFilms);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    IFilmResolver filmResolver,
    DataAccess dataAccess)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Put("/settings/favourite-films");
        Description(x => x.WithTags("Settings"));
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var lang = currentUserService.LangCulture;

        var filmIds = command.FavouriteFilms.Select(x => x.ExternalId).Distinct().ToList();
        
        await filmResolver.EnsureFilmsExistAsync(
            filmIds,
            lang,
            ct);

        var result = await dataAccess.UpdateFavoriteFilms(command, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}