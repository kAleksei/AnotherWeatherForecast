using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Common.Interfaces;

/// <summary>
/// Defines a contract for weather data source providers.
/// </summary>
public interface IWeatherSourceProvider
{
    /// <summary>
    /// Gets the name of the weather source this provider represents.
    /// Examples: "OpenMeteo", "WeatherAPI", "OpenWeatherMap"
    /// </summary>
    string SourceName { get; }

    /// <summary>
    /// Retrieves weather forecast data from the external source.
    /// </summary>
    /// <param name="location">The location for which to retrieve the forecast.</param>
    /// <param name="date">The date for the forecast.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A forecast source containing the weather data or error information.</returns>
    Task<ForecastSource> GetForecastAsync(Location location, DateTime date, CancellationToken cancellationToken);
}
