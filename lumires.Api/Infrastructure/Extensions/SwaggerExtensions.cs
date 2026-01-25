using FastEndpoints.Swagger;
using NSwag;
using Scalar.AspNetCore;

namespace lumires.Api.Infrastructure.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddLumiresSwagger(this IServiceCollection services)
    {
        services.SwaggerDocument(o =>
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
                    Description = "Paste your Supabase JWT access token here"
                });
            };

            o.ShortSchemaNames = true;
        });

        return services;
    }

    public static IApplicationBuilder UseLumiresSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment()) return app;

        app.UseSwaggerGen(options => { options.Path = "/openapi/{documentName}.json"; });

        if (app is WebApplication webApp)
            webApp.MapScalarApiReference(options =>
            {
                options.ForceDarkMode()
                    .WithTitle("Lumires API")
                    .AddPreferredSecuritySchemes("Bearer");
            });

        return app;
    }
}