namespace WeatherForecast.Application.Common.Models;

/// <summary>
/// Represents aggregated weather forecast data from multiple sources.
/// </summary>
public class AggregatedForecastDto
{
    /// <summary>
    /// Gets or sets the average temperature in Celsius from all available sources.
    /// </summary>
    public decimal? AverageTemperatureCelsius { get; set; }

    /// <summary>
    /// Gets or sets the average humidity percentage from all available sources.
    /// </summary>
    public decimal? AverageHumidityPercent { get; set; }

    /// <summary>
    /// Gets or sets the temperature range (min to max) from all available sources.
    /// </summary>
    public string? TemperatureRange { get; set; }

    /// <summary>
    /// Gets or sets the humidity range (min to max) from all available sources.
    /// </summary>
    public string? HumidityRange { get; set; }
}
