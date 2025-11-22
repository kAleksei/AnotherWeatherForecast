using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Domain.Interfaces;

/// <summary>
/// Defines the contract for weather forecast repository operations.
/// </summary>
public interface IWeatherRepository
{
    /// <summary>
    /// Gets the weather forecast for a specific location and date.
    /// </summary>
    /// <param name="location">The location to get the forecast for.</param>
    /// <param name="date">The date to get the forecast for.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The weather forecast if found; otherwise, null.</returns>
    Task<Entities.WeatherForecast?> GetForecastAsync(
        Location location,
        DateTime date,
        CancellationToken cancellationToken);
}
