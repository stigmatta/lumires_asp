using System.Diagnostics;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api;

[UsedImplicitly]
internal sealed partial class BenchmarkProcessor(ILogger<BenchmarkProcessor> logger)
    : IGlobalPreProcessor, IGlobalPostProcessor
{
    private const string TimerKey = "RequestStartTime";
    private readonly ILogger _logger = logger;

    public Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        Debug.Assert(context != null, nameof(context) + " != null");

        if (!context.HttpContext.Items.TryGetValue(TimerKey, out var startObj) || startObj is not long startTimestamp)
            return Task.CompletedTask;
        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

        var endpoint = context.HttpContext.GetEndpoint();
        var endpointName = endpoint?.DisplayName ?? "UnknownEndpoint";
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;

        LogBenchmark(_logger, method, path, endpointName, (long)elapsed.TotalMilliseconds);

        return Task.CompletedTask;
    }

    public Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        Debug.Assert(context != null, nameof(context) + " != null");
        context.HttpContext.Items[TimerKey] = Stopwatch.GetTimestamp();
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{Method} {Path} ({EndpointName}) executed in {Elapsed} ms")]
    static partial void LogBenchmark(ILogger logger, string method, string path, string endpointName, long elapsed);
}