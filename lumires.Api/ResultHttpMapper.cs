using System.Diagnostics;
using Ardalis.Result;
using FastEndpoints;

namespace lumires.Api;

public static class ResultHttpMapper
{
    public static async Task SendErrorAsync<TValue>(
        this HttpContext httpContext,
        Result<TValue> result,
        CancellationToken ct)
    {
        Debug.Assert(result is not null,
            "Result is not null according to nullable reference types' annotations ");

        if (result.IsSuccess) return;

        Debug.Assert(httpContext is not null,
            "Result is not null according to nullable reference types' annotations ");
        switch (result.Status)
        {
            case ResultStatus.NotFound:
                await httpContext.Response.SendNotFoundAsync(ct);
                break;
            case ResultStatus.Unauthorized:
                await httpContext.Response.SendUnauthorizedAsync(ct);
                break;
            case ResultStatus.Forbidden:
                await httpContext.Response.SendForbiddenAsync(ct);
                break;
            default:
                await httpContext.Response.SendAsync(
                    new ProblemDetails
                    {
                        Status = 500,
                        Detail = string.Join(", ", result.Errors)
                    },
                    500,
                    cancellation: ct);
                break;
        }
    }
}