using Core.Abstractions.Data;
using Core.Abstractions.Services;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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