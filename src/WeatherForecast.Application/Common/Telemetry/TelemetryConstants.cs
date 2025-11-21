using System.Diagnostics;

namespace WeatherForecast.Application.Common.Telemetry;

public static class TelemetryConstants
{
    public static readonly ActivitySource ActivitySource = new("WeatherForecast.Application");
}