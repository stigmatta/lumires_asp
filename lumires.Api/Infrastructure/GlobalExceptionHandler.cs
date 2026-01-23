using System.Diagnostics;
using System.Net;
using lumires.Api.Resources;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Infrastructure;

internal partial class GlobalExceptionHandler(
    IStringLocalizer<SharedResource> localizer,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "[{TraceId}] An unhandled exception occurred: {Message}")]
    static partial void LogUnhandledException(ILogger logger, Exception ex, string traceId, string message);

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        LogUnhandledException(logger, exception, traceId, exception.Message);

        var (statusCode, messageKey) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Error_NotFound"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Error_Unauthorized"),
            _ => (HttpStatusCode.InternalServerError, "Error_Internal")
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = localizer["Error_Title"],
            Detail = localizer[messageKey],
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions.Add("traceId", traceId);

        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync<ProblemDetails>(
            value: problemDetails, 
            cancellationToken: ct)
            .ConfigureAwait(false);

        return true;
    }
}