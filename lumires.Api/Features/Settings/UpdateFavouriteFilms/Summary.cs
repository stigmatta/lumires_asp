using FastEndpoints;
using JetBrains.Annotations;

namespace lumires.Api.Features.Settings.UpdateFavouriteFilms;

[UsedImplicitly]
internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "UpdateFavouriteFilms";
        Description = """
                      Updates favourite films of self.

                      If films are not in the db - enrich them with a resolver.

                      Can return 403 if jwt is not corresponding with a current user

                      ### Notes

                      - **Authorization Bearer** — Is required
                      """;

        ExampleRequest = new Command(
            [
                new FavouriteFilm(550, 1), new FavouriteFilm(9741, 2), new FavouriteFilm(263115, 3),
                new FavouriteFilm(8844, 4)
            ]
        );
        Response(204, "Favourite films are successfully updated");
        Response(400);
    }
}