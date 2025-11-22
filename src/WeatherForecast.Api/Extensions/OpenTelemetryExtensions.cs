using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace WeatherForecast.Api.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry observability.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry instrumentation and exporters to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The host environment.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Only configure OpenTelemetry if OTLP endpoint is provided
        var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");
        if (string.IsNullOrEmpty(otlpEndpoint))
        {
            return services;
        }

        var serviceName = configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "WeatherForecast.Api";
        
        // Calculate version from assembly if not provided
        var serviceVersion = configuration.GetValue<string>("OpenTelemetry:ServiceVersion");
        if (string.IsNullOrEmpty(serviceVersion))
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                         ?? assembly.GetName().Version?.ToString()
                         ?? "1.0.0";
            serviceVersion = version;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext =>
                        {
                            // Don't trace health check endpoints
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddRedisInstrumentation(connection =>
                    {
                        connection.SetVerboseDatabaseStatements = true;
                    });

                // Add console exporter in development
                if (environment.IsDevelopment())
                {
                    tracing.AddConsoleExporter();
                }

                // Add OTLP exporter
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                // Add console exporter in development
                if (environment.IsDevelopment())
                {
                    metrics.AddConsoleExporter();
                }

                // Add OTLP exporter
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            });

        return services;
    }
}
