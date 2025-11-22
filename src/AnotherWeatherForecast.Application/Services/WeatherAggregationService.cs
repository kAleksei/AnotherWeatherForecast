using AnotherWeatherForecast.Application.Common.Interfaces;
using AnotherWeatherForecast.Application.Common.Models;
using Microsoft.Extensions.Logging;
using AnotherWeatherForecast.Application.Common.Extensions;
using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Application.Services;

/// <summary>
/// Service responsible for aggregating weather forecast data from multiple sources.
/// </summary>
public class WeatherAggregationService : IWeatherAggregationService
{
    private readonly IEnumerable<IWeatherSourceProvider> _weatherProviders;
    private readonly ILogger<WeatherAggregationService> _logger;

    public WeatherAggregationService(
        IEnumerable<IWeatherSourceProvider> weatherProviders,
        ILogger<WeatherAggregationService> logger)
    {
        _weatherProviders = weatherProviders ?? throw new ArgumentNullException(nameof(weatherProviders));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<WeatherForecastResponse> GetAggregatedForecastAsync(
        WeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching weather forecast for {City}, {Country} on {Date}",
            request.City, request.Country, request.Date.ToString("yyyy-MM-dd"));

        var providersToQuery = FilterProviders(request.Sources);
        if (!providersToQuery.Any())
        {
            _logger.LogInformation("No matching weather source providers found for the requested sources. Sources: {Sources}",
                request.Sources != null ? string.Join(", ", request.Sources) : null);
            
        }
        
        var location = new Location(request.City, request.Country);

        var forecastTasks = providersToQuery.Select(async provider =>
            await FetchFromProviderAsync(provider, location, request.Date, cancellationToken));

        var forecastSources = await Task.WhenAll(forecastTasks);
        var aggregatedForecast = CalculateAggregatedForecast(forecastSources);

        var response = new WeatherForecastResponse
        {
            Location = location.ToLocationString(),
            Date = request.Date,
            AggregatedForecast = aggregatedForecast,
            Sources = forecastSources.Select(s => s.ToDto()).ToList()
        };

        _logger.LogInformation(
            "Weather forecast aggregated for {City}, {Country}. Available: {AvailableCount}/{TotalCount}",
            request.City,
            request.Country,
            forecastSources.Count(s => s.Available),
            forecastSources.Length);

        return response;
    }

    private IEnumerable<IWeatherSourceProvider> FilterProviders(List<string>? requestedSources)
    {
        if (requestedSources == null || requestedSources.Count == 0)
        {
            return _weatherProviders;
        }

        var filteredProviders = _weatherProviders
            .Where(p => requestedSources.Contains(p.SourceName, StringComparer.OrdinalIgnoreCase))
            .ToList();
        
        return filteredProviders;
    }

    private async Task<ForecastSource> FetchFromProviderAsync(
        IWeatherSourceProvider provider,
        Location location,
        DateTime date,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await provider.GetForecastAsync(location, date, cancellationToken);
            
            if (!result.Available)
            {
                _logger.LogWarning("{SourceName} is unavailable: {Error}", provider.SourceName, result.Error);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching forecast from {SourceName}. Exception: {Error}", provider.SourceName, ex.Message);
            
            return new ForecastSource(
                provider.SourceName,
                null,
                null,
                false,
                $"Exception: {ex.Message}",
                DateTime.UtcNow);
        }
    }

    private AggregatedForecastDto? CalculateAggregatedForecast(ForecastSource[] sources)
    {
        var availableSources = sources.Where(s => s.Available && s.Temperature != null && s.Humidity != null).ToList();

        if (availableSources.Count == 0)
        {
            _logger.LogWarning("No available sources with valid data for aggregation");
            return null;
        }

        var temperatures = availableSources.Select(s => s.Temperature!.Celsius).ToList();
        var humidities = availableSources.Select(s => s.Humidity!.Percent).ToList();

        var avgTemperature = temperatures.Average();
        var avgHumidity = humidities.Average();

        var minTemp = temperatures.Min();
        var maxTemp = temperatures.Max();
        var minHumidity = humidities.Min();
        var maxHumidity = humidities.Max();

        return new AggregatedForecastDto
        {
            AverageTemperatureCelsius = Math.Round(avgTemperature, 1),
            AverageHumidityPercent = Math.Round(avgHumidity, 1),
            TemperatureRange = minTemp == maxTemp 
                ? $"{minTemp:F1}°C" 
                : $"{minTemp:F1}°C - {maxTemp:F1}°C",
            HumidityRange = minHumidity == maxHumidity 
                ? $"{minHumidity:F1}%" 
                : $"{minHumidity:F1}% - {maxHumidity:F1}%"
        };
    }
}
