using System.Diagnostics;
using System.Globalization;
using lumires.Api.Core.Options;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

namespace lumires.Api.Infrastructure.Extensions;

internal static class LoggerExtensions
{
    public static void AddLumiresLogging(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var logtailConfig = configuration
            .GetSection(LogtailConfig.SectionName)
            .Get<LogtailConfig>() ?? new LogtailConfig();
        

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information() 
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Http", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .MinimumLevel.Override("FastEndpoints", LogEventLevel.Information)  
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .Enrich.WithProperty("App", "Lumires.Api")
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                path: "logs/lumires_log-.txt",
                rollingInterval: RollingInterval.Day,
                formatProvider: CultureInfo.InvariantCulture
            )
            .WriteTo.BetterStack(
                sourceToken: logtailConfig.ApiKey,
                betterStackEndpoint: logtailConfig.BaseUrl.ToString())
            .CreateLogger();

        builder.Host.UseSerilog();
    }

}