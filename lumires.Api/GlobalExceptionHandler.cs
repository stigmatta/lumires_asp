using System.Diagnostics;
using System.Net;
using System.Security;
using Contracts.Resources;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace lumires.Api;

internal sealed partial class GlobalExceptionHandler(
    IStringLocalizer<SharedResource> localizer,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        LogUnhandledException(logger, exception, traceId, exception.Message);

        var (statusCode, messageKey) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Error_Unauthorized"),
            SecurityException => (HttpStatusCode.Forbidden, "Error_Forbidden"),
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

        await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                ct)
            .ConfigureAwait(false);

        return true;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "[{TraceId}] Exception: {Message}")]
    static partial void LogUnhandledException(ILogger logger, Exception ex, string traceId, string message);
}