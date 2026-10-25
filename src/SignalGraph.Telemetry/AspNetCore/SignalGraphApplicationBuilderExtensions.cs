using Microsoft.AspNetCore.Builder;

namespace SignalGraph.Telemetry.AspNetCore;

/// <summary>
/// ASP.NET Core pipeline helpers.
/// </summary>
public static class SignalGraphApplicationBuilderExtensions
{
    /// <summary>
    /// Adds request-scoped SignalGraph operations (what/when/why for each HTTP request).
    /// Call after routing if you want endpoint metadata available later; order is otherwise flexible.
    /// </summary>
    public static IApplicationBuilder UseSignalGraphRequestTelemetry(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<SignalGraphRequestMiddleware>();
    }
}
