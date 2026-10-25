using Microsoft.AspNetCore.Http;

namespace SignalGraph.Telemetry.AspNetCore;

/// <summary>
/// Starts one SignalGraph operation per HTTP request so handlers share span context for exceptions.
/// </summary>
public sealed class SignalGraphRequestMiddleware
{
    private readonly RequestDelegate _next;

    public SignalGraphRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISignalGraph signals)
    {
        var name = $"{context.Request.Method} {context.Request.Path}";

        using var operation = signals.StartOperation(name, o => o
            .Attribute("http.request.method", context.Request.Method)
            .Attribute("url.path", context.Request.Path.Value));

        try
        {
            await _next(context).ConfigureAwait(false);

            operation.SetAttribute("http.response.status_code", context.Response.StatusCode);

            if (context.Response.StatusCode >= 500)
            {
                operation.SetStatus(OperationStatus.Error, $"HTTP {context.Response.StatusCode}");
            }
            else
            {
                operation.SetStatus(OperationStatus.Ok);
            }
        }
        catch (Exception ex)
        {
            operation.RecordException(ex);
            throw;
        }
    }
}
