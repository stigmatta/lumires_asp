using System.Globalization;
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

namespace Infrastructure.Extensions;

internal static class LoggerExtensions
{
    public static void AddCustomLogging(
        this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

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
            .Enrich.WithProperty("App", "Lumires.lumires.Api")
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.BetterStack(
                logtailConfig.ApiKey,
                logtailConfig.BaseUrl.ToString())
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }
}
