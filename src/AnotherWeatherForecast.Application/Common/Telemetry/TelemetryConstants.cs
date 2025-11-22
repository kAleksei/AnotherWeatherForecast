using System.Diagnostics;

namespace AnotherWeatherForecast.Application.Common.Telemetry;

/// <summary>
/// Provides centralized telemetry constants for distributed tracing.
/// </summary>
public static class TelemetryConstants
{
    public static readonly ActivitySource ActivitySource = new("AnotherWeatherForecast.Application");
}