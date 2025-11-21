namespace WeatherForecast.Application.Common.Models;

/// <summary>
/// Represents the response containing weather forecast data.
/// </summary>
public class WeatherForecastResponse
{
    /// <summary>
    /// Gets or sets the location information (city, country).
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the forecast date (ISO 8601 format).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the aggregated forecast data from all sources.
    /// </summary>
    public AggregatedForecastDto? AggregatedForecast { get; set; }

    /// <summary>
    /// Gets or sets the list of individual forecast sources with their data.
    /// </summary>
    public List<ForecastSourceDto> Sources { get; set; } = new();
}