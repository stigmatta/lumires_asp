using System.Diagnostics;
using System.Reflection;
using FastEndpoints;
using FastEndpoints.Swagger;
using lumires.Core.Abstractions.Data;
using NSwag;
using Scalar.AspNetCore;
using ServiceDefaults;

namespace lumires.Api;

public static class ApiRegistration
{
    public static IHostApplicationBuilder AddApi(
        this IHostApplicationBuilder builder)
    {
        Debug.Assert(builder != null, nameof(builder) + " != null");

        builder.Services.AddFastEndpoints()
            .AddResponseCaching();
        builder.Services.RegisterQueryClasses();

        builder.AddServiceDefaults();

        builder.Services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "Lumires API";
                s.Version = "v1";

                s.AddAuth("Bearer", new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Paste your JWT access token here"
                });
            };

            o.ShortSchemaNames = true;
        });


        var supportedCultures = new[] { "uk-UA", "en-US" };
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture("en-US")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
        });

        builder.Services.AddLocalization();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                    context.ProblemDetails.Extensions.Add("traceId",
                        Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            };
        });

        return builder;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseRequestLocalization();
        app.UseExceptionHandler();

        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints(config =>
        {
            config.Endpoints.Configurator = ep =>
            {
                ep.PreProcessor<BenchmarkProcessor>(Order.Before);
                ep.PostProcessor<BenchmarkProcessor>(Order.After);
            };
        });

        Debug.Assert(app != null, nameof(app) + " != null");

        app.UseSwaggerGen(options => { options.Path = "/openapi/{documentName}.json"; });

        app.MapScalarApiReference(options =>
        {
            options.ForceDarkMode()
                .WithTitle("Lumires API")
                .AddPreferredSecuritySchemes("Bearer");
        });
        app.Lifetime.ApplicationStarted.Register(() => OpenScalar(app));

        return app;
    }


    private static IServiceCollection RegisterQueryClasses(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var queryTypes = assembly.GetTypes()
            .Where(t => typeof(IDataAccess).IsAssignableFrom(t)
                        && t is { IsClass: true, IsAbstract: false });

        foreach (var type in queryTypes) services.AddScoped(type);

        return services;
    }

    private static void OpenScalar(WebApplication app)
    {
        var address = app.Urls.FirstOrDefault(u => u.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                      ?? app.Urls.FirstOrDefault(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase));

        if (address != null)
            Process.Start(new ProcessStartInfo
            {
                FileName = $"{address.TrimEnd('/')}/scalar",
                UseShellExecute = true
            });
    }
}