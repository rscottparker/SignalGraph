using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SignalGraph.Telemetry;

/// <summary>
/// DI registration for SignalGraph telemetry.
/// </summary>
public static class SignalGraphServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ISignalGraph"/> and binds <see cref="SignalGraphOptions"/>.
    /// Configure an OpenTelemetry tracer provider in the host to export spans
    /// (add the ActivitySource named by <see cref="SignalGraphOptions.ServiceName"/>).
    /// </summary>
    public static IServiceCollection AddSignalGraph(
        this IServiceCollection services,
        Action<SignalGraphOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<SignalGraphOptions>();
        }

        services.TryAddSingleton<ISignalGraph, SignalGraphTelemetry>();
        return services;
    }
}
