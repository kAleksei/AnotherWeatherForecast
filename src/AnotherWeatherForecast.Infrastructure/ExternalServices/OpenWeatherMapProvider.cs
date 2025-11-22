using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using AnotherWeatherForecast.Application.Common.Interfaces;
using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;
using AnotherWeatherForecast.Infrastructure.ExternalServices.OpenWeatherMap;

namespace AnotherWeatherForecast.Infrastructure.ExternalServices;

/// <summary>
/// Weather source provider for OpenWeatherMap (requires API key).
/// Acts as a mediator that delegates to historical or forecast providers based on the requested date.
/// </summary>
public class OpenWeatherMapProvider : IWeatherSourceProvider
{
    private readonly IOpenWeatherMapHistoricalProvider _historicalProvider;
    private readonly IOpenWeatherMapForecastProvider _forecastProvider;
    private readonly ILogger<OpenWeatherMapProvider> _logger;

    public const string SourceProviderName = "OpenWeatherMap";
    public string SourceName => SourceProviderName;

    public OpenWeatherMapProvider(
        [FromKeyedServices("OpenWeatherMap.Historical")] IOpenWeatherMapHistoricalProvider historicalProvider,
        [FromKeyedServices("OpenWeatherMap.Forecast")] IOpenWeatherMapForecastProvider forecastProvider,
        ILogger<OpenWeatherMapProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(historicalProvider);
        ArgumentNullException.ThrowIfNull(forecastProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _historicalProvider = historicalProvider;
        _forecastProvider = forecastProvider;
        _logger = logger;
    }

    public async Task<ForecastSource> GetForecastAsync(
        Location location,
        DateTime date,
        CancellationToken cancellationToken)
    {
        try
        {
            var daysDiff = (date.Date - DateTime.UtcNow.Date).Days;

            if (daysDiff <= 0)
            {
                return await _historicalProvider.GetHistoricalForecastAsync(
                    location, date, SourceName, cancellationToken);
            }
            else
            {
                return await _forecastProvider.GetDailyForecastAsync(
                    location, date, SourceName, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or JsonException or InvalidOperationException or BrokenCircuitException)
        {
            _logger.LogError(ex, "Error fetching weather from {SourceName}", SourceName);
            return new ForecastSource(SourceName, null, null, false, ex.Message, DateTime.UtcNow);
        }
    }
}
