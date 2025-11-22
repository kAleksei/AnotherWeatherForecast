using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace AnotherWeatherForecast.Api.Extensions;

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

        var serviceName = configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? "AnotherWeatherForecast.Api";
        
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

        var headers = configuration.GetValue<string>("OpenTelemetry:OtlpHeaders") ?? string.Empty;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options => { options.RecordException = true; })
                    .AddHttpClientInstrumentation(options => { options.RecordException = true; })
                    .SetErrorStatusOnException()
                    .AddSource(
                        "Microsoft.AspNetCore.*",
                        "AnotherWeatherForecast.*");

                // Add console exporter in development
                if (environment.IsDevelopment())
                {
                    tracing.AddConsoleExporter();
                }

                // Add OTLP exporter
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri($"{otlpEndpoint}/traces");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.*",
                        "AnotherWeatherForecast.*");

                // Add console exporter in development
                if (environment.IsDevelopment())
                {
                    metrics.AddConsoleExporter();
                }

                // Add OTLP exporter
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri($"{otlpEndpoint}/metrics");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            })
            .WithLogging(logging =>
            {
                logging.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri($"{otlpEndpoint}/logs");
                    options.Headers = headers;
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });

        return services;
    }
}
