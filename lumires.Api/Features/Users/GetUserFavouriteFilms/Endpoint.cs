using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Users.GetUserFavouriteFilms;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record FavouriteFilm(int Id, string Title, string? PosterPath, int? ReleaseYear, string[] Genres, float VoteAverage);

[UsedImplicitly]
internal sealed record Response(IReadOnlyCollection<FavouriteFilm> FavouriteFilms);

internal sealed class Endpoint(
    DataAccess db,
    ICurrentUserService currentUserService)
    : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/users/{username}/favourite-films");
        Description(x => x.WithTags("Users"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var lang = currentUserService.LangCulture;
        var result = await db.GetFavouriteFilms(query.Username, lang, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }
        await Send.OkAsync(result, ct);
    }
}