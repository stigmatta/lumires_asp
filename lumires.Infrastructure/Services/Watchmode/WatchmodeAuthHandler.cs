using System.Web;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Watchmode;

internal sealed class WatchmodeAuthHandler(IOptions<WatchmodeOptions> options) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var uriBuilder = new UriBuilder(request.RequestUri!);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query["apiKey"] = options.Value.ApiKey;

        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;

        return await base.SendAsync(request, ct);
    }
}