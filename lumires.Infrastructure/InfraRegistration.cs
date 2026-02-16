using System.Diagnostics;
using Core.Abstractions.Data;
using Core.Abstractions.Services;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class InfraRegistration
{
    public static IHostApplicationBuilder AddInfrastructure(
        this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;
        var env = builder.Environment;
        
        builder.Services.AddScoped<IAppDbContext>(provider => 
            provider.GetRequiredService<AppDbContext>());

        builder.AddNpgsqlDbContext<AppDbContext>("supabaseDB", null, options =>
        {
            options.UseNpgsql(npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly("lumires.Infrastructure"));
        });
        builder.Services.AddHealthChecks()
            .AddNpgSql(builder.Configuration.GetConnectionString("supabaseDB")!);

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
        
        if (app.Environment.IsDevelopment())
        {
            app.Lifetime.ApplicationStarted.Register(OpenLogtailDashboard);
        }

        return app;
    }
    
    
    private static void OpenLogtailDashboard()
    {
        try
        {
            const string url = "https://telemetry.betterstack.com/team/t498261/tail?s=1694678";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to open Logtail URL: " + ex.Message);
        }
    }
}