using lumires.Api.Infrastructure.Hubs;
using Microsoft.AspNetCore.Http.Connections;

namespace lumires.Api.Infrastructure.Extensions;

public static class SignalRExtensions
{
    public static void MapLumiresHubs(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var hubUrl = configuration["SignalRSettings:HubUrl"] ?? "/hubs/notifications";
        endpoints.MapHub<NotificationHub>(hubUrl, options => { options.Transports = HttpTransportType.WebSockets; });
    }
}