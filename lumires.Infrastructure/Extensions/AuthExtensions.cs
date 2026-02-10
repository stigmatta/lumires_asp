using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Core.Auth;
using Core.Resources;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Extensions;

internal static partial class AuthExtensions
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to parse Supabase app_metadata JSON for user {UserId}")]
    static partial void LogMetadataParseError(ILogger logger, string? userId, Exception? ex);

    public static IServiceCollection AddCustomAuth(this IServiceCollection services, IConfiguration config)
    {
        var projectUrl = config["Supabase:Url"]
                         ?? throw new InvalidOperationException("Supabase URL is missing!");
        var hubUrl = config["SignalR:HubUrl"];

        services.AddSingleton<IAuthorizationHandler, CustomAuthorizationHandler>();

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
                    OnTokenValidated = context =>
                    {
                        var appMetadata = context.Principal?.FindFirst("app_metadata")?.Value;
                        if (string.IsNullOrEmpty(appMetadata)) return Task.CompletedTask;

                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("Auth.JwtBearer");  
                        try
                        {
                            using var db = JsonDocument.Parse(appMetadata);
                            var claims = new List<Claim>();

                            if (db.RootElement.TryGetProperty("role", out var r))
                                claims.Add(new Claim("role", r.GetString()!));

                            if (db.RootElement.TryGetProperty("tier", out var t))
                                claims.Add(new Claim("tier", t.GetString()!));

                            if (claims.Count > 0)
                            {
                                var appIdentity = new ClaimsIdentity(claims);
                                context.Principal?.AddIdentity(appIdentity);
                            }
                        }
                        catch (JsonException ex)
                        {
                            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            LogMetadataParseError(logger, userId, ex);
                        }

                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.Request.Path.StartsWithSegments(hubUrl, StringComparison.OrdinalIgnoreCase))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = async context =>
                    {
                        var localizer = context.HttpContext.RequestServices
                            .GetRequiredService<IStringLocalizer<SharedResource>>();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        var problemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = localizer["Error_Title"],
                            Detail = localizer["Error_Unauthorized"],
                            Instance = context.HttpContext.Request.Path
                        };

                        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                        problemDetails.Extensions.Add("traceId", traceId);

                        await context.Response.WriteAsJsonAsync(problemDetails);
                    },
                    OnForbidden = async context =>
                    {
                        var localizer = context.HttpContext.RequestServices
                            .GetRequiredService<IStringLocalizer<SharedResource>>();

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/problem+json";

                        var problemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status403Forbidden,
                            Title = localizer["Error_Title"],
                            Detail = localizer["Error_Forbidden"],
                            Instance = context.Request.Path
                        };

                        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                        problemDetails.Extensions.Add("traceId", traceId);

                        await context.Response.WriteAsJsonAsync(problemDetails);
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(CustomPolicies.AdminOnly, policy =>
                policy.AddRequirements(new CustomRequirement(UserRoles.Admin, UserTiers.Free)))
            .AddPolicy(CustomPolicies.StaffOnly, policy =>
                policy.AddRequirements(new CustomRequirement(UserRoles.Moderator, UserTiers.Free)))
            .AddPolicy(CustomPolicies.TierOnly, policy =>
                policy.AddRequirements(new CustomRequirement(UserRoles.User, UserTiers.Pro)))
            .AddPolicy(CustomPolicies.PatronOnly, policy =>
                policy.AddRequirements(new CustomRequirement(UserRoles.User, UserTiers.Patron)))
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("tier", UserTiers.Free, UserTiers.Pro, UserTiers.Patron)
                .Build());

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:3000", //TODO CHANGE
                        "http://127.0.0.1:5500",
                        "http://localhost:5500"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}