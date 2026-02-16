using Infrastructure.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

internal static class SignalRExtensions
{
    public static void MapCustomHubs(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var hubUrl = configuration["SignalRSettings:HubUrl"] ?? "/hubs/notifications";
        endpoints.MapHub<NotificationHub>(hubUrl, options => { options.Transports = HttpTransportType.WebSockets; });
    }
}