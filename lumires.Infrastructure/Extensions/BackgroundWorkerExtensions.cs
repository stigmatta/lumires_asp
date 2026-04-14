using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;

namespace Infrastructure.Extensions;

internal static class BackgroundWorkerExtensions
{
    public static IServiceCollection AddWorker(this IServiceCollection services)
    {
        services.AddTickerQ(options => { options.AddDashboard(); });

        return services;
    }
}