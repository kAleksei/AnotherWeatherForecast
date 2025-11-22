using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Infrastructure.ExternalServices.OpenWeatherMap;

/// <summary>
/// Interface for OpenWeatherMap historical weather data provider.
/// </summary>
public interface IOpenWeatherMapHistoricalProvider
{
    /// <summary>
    /// Gets historical weather forecast for a specific location and date.
    /// </summary>
    /// <param name="location">The location to get the forecast for.</param>
    /// <param name="date">The date to get the forecast for (past date).</param>
    /// <param name="sourceName">The name of the source provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The forecast source with historical weather data.</returns>
    Task<ForecastSource> GetHistoricalForecastAsync(
        Location location,
        DateTime date,
        string sourceName,
        CancellationToken cancellationToken);
}
