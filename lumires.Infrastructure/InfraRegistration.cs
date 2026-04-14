using System.Diagnostics;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;
using TickerQ.DependencyInjection;

namespace Infrastructure;

public static class InfraRegistration
{
    public static IHostApplicationBuilder AddInfrastructure(
        this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;
        var env = builder.Environment;


        var dbString = builder.Configuration.GetConnectionString("db");

        builder.Services.AddScoped<IAppDbContext>(provider =>
            provider.GetRequiredService<AppDbContext>());

        builder.AddNpgsqlDbContext<AppDbContext>("db", settings => { settings.ConnectionString = dbString; },
            options =>
            {
                options.UseNpgsql(npgsqlOptions =>
                    npgsqlOptions.MigrationsAssembly("lumires.Infrastructure"));
            });
        builder.Services.AddHealthChecks()
            .AddNpgSql(dbString
                       ?? throw new InvalidOperationException("db connection string not found"));

        // Auth
        services.AddCustomAuth(config);

        //Logging
        builder.AddCustomLogging();

        // Caching
        services.AddCaching(config);

        // SignalR
        services.AddSignalR();

        // Email
        services.AddEmailSender(config, env);

        // External APIs
        services.AddExternalApis(config);

        //Background worker
        services.AddWorker();

        // Scoped
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<INotificationService, NotificationService>();


        return builder;
    }

    public static IApplicationBuilder UseInfrastructure(
        this WebApplication app)
    {
        app.MapCustomHubs(app.Configuration);

        var host = app.Urls.FirstOrDefault();
        app.Lifetime.ApplicationStarted.Register(() => OpenDashboards(app));

        app.UseTickerQ();

        return app;
    }


    private static void OpenDashboards(WebApplication app)
    {
        var address = app.Urls.FirstOrDefault(u => u.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                      ?? app.Urls.FirstOrDefault(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase));

        if (address is null) return;

        var baseUrl = address.TrimEnd('/');

        foreach (var url in new[]
                 {
                     $"{baseUrl}/tickerq/dashboard",
                     "https://telemetry.betterstack.com/team/t498261/tail?s=1694678"
                 })
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open {url}: {ex.Message}");
            }
    }
}