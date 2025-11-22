using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Infrastructure.ExternalServices.OpenWeatherMap;

/// <summary>
/// Interface for OpenWeatherMap forecast data provider.
/// </summary>
public interface IOpenWeatherMapForecastProvider
{
    /// <summary>
    /// Gets daily weather forecast for a specific location and date.
    /// </summary>
    /// <param name="location">The location to get the forecast for.</param>
    /// <param name="date">The date to get the forecast for (future date, up to 5 days).</param>
    /// <param name="sourceName">The name of the source provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The forecast source with daily weather forecast data.</returns>
    Task<ForecastSource> GetDailyForecastAsync(
        Location location,
        DateTime date,
        string sourceName,
        CancellationToken cancellationToken);
}
