using AnotherWeatherForecast.Application.Common.Models;

namespace AnotherWeatherForecast.Application.Common.Interfaces;

/// <summary>
/// Defines a contract for the weather aggregation service.
/// </summary>
public interface IWeatherAggregationService
{
    /// <summary>
    /// Retrieves and aggregates weather forecast data from multiple sources.
    /// </summary>
    /// <param name="request">The weather forecast request containing date, location, and optional source filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An aggregated weather forecast response with data from all queried sources.</returns>
    Task<WeatherForecastResponse> GetAggregatedForecastAsync(WeatherForecastRequest request, CancellationToken cancellationToken);
}
