using System.Net.Http.Headers;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Tmdb;

internal sealed class TmdbAuthHandler(IOptions<TmdbConfig> options) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = options.Value.BearerToken;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}