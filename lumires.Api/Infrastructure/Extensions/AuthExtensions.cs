using lumires.Api.Infrastructure.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace lumires.Api.Infrastructure.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddLumiresAuth(this IServiceCollection services, IConfiguration config)
    {
        var projectUrl = config["Supabase:Url"]
                         ?? throw new InvalidOperationException("Supabase URL is missing!");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{projectUrl.TrimEnd('/')}/auth/v1";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = null,

                    ValidateIssuer = true,
                    ValidIssuer = $"{projectUrl.TrimEnd('/')}/auth/v1",

                    ValidateAudience = true,
                    ValidAudience = "authenticated",

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.Request.Path.StartsWithSegments(HubConstants.Notifications))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Auth failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}