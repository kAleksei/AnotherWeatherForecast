namespace WeatherForecast.Application.Common.Models;

/// <summary>
/// Represents a request for weather forecast data.
/// </summary>
public class WeatherForecastRequest
{
    /// <summary>
    /// Gets or sets the date for the forecast (ISO 8601 format).
    /// Must be within +7 days from today, no limitation for past dates.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country code (ISO 3166-1 alpha-2, e.g., 'US', 'GB').
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional list of weather source names to query.
    /// If not specified, all available sources will be queried.
    /// Examples: "OpenMeteo", "WeatherAPI", "OpenWeatherMap"
    /// </summary>
    public List<string>? Sources { get; set; }
}
