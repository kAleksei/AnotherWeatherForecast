using System.Diagnostics;

namespace WeatherForecast.Application.Common.Telemetry;

/// <summary>
/// Provides centralized telemetry constants for distributed tracing.
/// </summary>
public static class TelemetryConstants
{
    public static readonly ActivitySource ActivitySource = new("WeatherForecast.Application");
}