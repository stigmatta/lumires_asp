using Infrastructure.BackgroundJobs;
using Infrastructure.Hubs;
using Infrastructure.Services;
using lumires.Core.Abstractions.Services;
using Microsoft.AspNetCore.Http.Connections;

namespace Infrastructure.Extensions;

internal static class NotificationsExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddScoped<INotificationService, OutboxService>();

        services.AddScoped<NotificationService>();

        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    public static void MapCustomHubs(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        var hubUrl = configuration["SignalRSettings:HubUrl"] ?? "/hubs/notifications";
        endpoints.MapHub<NotificationHub>(hubUrl, options => { options.Transports = HttpTransportType.WebSockets; });
    }
}