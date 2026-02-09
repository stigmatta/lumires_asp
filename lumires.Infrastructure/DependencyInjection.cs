using Contracts.Abstractions;
using Infrastructure.Extensions;
using Infrastructure.Services;
using lumires.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(
        this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;
        var env = builder.Environment;

        builder.AddNpgsqlDbContext<AppDbContext>("supabaseDB", null, options =>
        {
            options.UseNpgsql(npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly("lumires.Domain"));
        });

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

        return app;
    }
}