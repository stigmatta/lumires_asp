namespace lumires.Api.Features.Films.GetFilms;

internal interface IGetFilms // For unit tests
{
    internal Task<List<FilmItemResponse>> GetFilmsAsync(Query query, string lang, CancellationToken ct);
    internal Task<int> GetFilmsCountAsync(Query query, string lang, CancellationToken ct);
}