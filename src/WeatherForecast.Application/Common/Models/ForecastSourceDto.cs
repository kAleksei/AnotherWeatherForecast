using Microsoft.AspNetCore.Mvc;

namespace WeatherForecast.Application.Common.Models;

/// <summary>
/// Represents weather forecast data from a specific source.
/// </summary>
public class ForecastSourceDto
{
    /// <summary>
    /// Gets or sets the name of the weather data source.
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temperature in Celsius from this source.
    /// </summary>
    public decimal? TemperatureCelsius { get; set; }

    /// <summary>
    /// Gets or sets the humidity percentage from this source.
    /// </summary>
    public decimal? HumidityPercent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this source is available and returned valid data.
    /// </summary>
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets the problem details if the source was not available (RFC 7807).
    /// </summary>
    public ProblemDetails? ProblemDetails { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the data was retrieved (UTC).
    /// </summary>
    public DateTime RetrievedAt { get; set; }
}
