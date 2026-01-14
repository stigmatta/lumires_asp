using lumires.Api.Shared.Abstractions;
using lumires.Api.Shared.Models;

namespace lumires.Api.Infrastructure.Services;

public class TmdbService(HttpClient httpClient) : IExternalMovieService
{
    public async Task<MovieImportResult?> GetMovieDetailsAsync(int movieId, CancellationToken ct = default)
    {
        var url = $"movie/{movieId}?append_to_response=credits,videos";

        var response = await httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<MovieImportResult>(ct);
    }
}