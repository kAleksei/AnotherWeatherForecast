using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Common.Extensions;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Application.Common.Models;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Interfaces;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Services;

/// <summary>
/// Service responsible for aggregating weather forecast data from multiple sources.
/// </summary>
public class WeatherAggregationService : IWeatherAggregationService
{
    private readonly IEnumerable<IWeatherSourceProvider> _weatherProviders;
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<WeatherAggregationService> _logger;

    public WeatherAggregationService(
        IEnumerable<IWeatherSourceProvider> weatherProviders,
        ICacheRepository cacheRepository,
        ILogger<WeatherAggregationService> logger)
    {
        _weatherProviders = weatherProviders ?? throw new ArgumentNullException(nameof(weatherProviders));
        _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<WeatherForecastResponse> GetAggregatedForecastAsync(
        WeatherForecastRequest request,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyGenerator.GenerateKey(
            request.City,
            request.Country,
            request.Date,
            request.Sources);

        _logger.LogDebug(
            "Fetching weather forecast for {City}, {Country} on {Date}. Cache key: {CacheKey}",
            request.City, request.Country, request.Date.ToString("yyyy-MM-dd"), cacheKey);

        var cachedResponse = await _cacheRepository.GetAsync<WeatherForecastResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        var providersToQuery = FilterProviders(request.Sources);
        var location = new Location(request.City, request.Country);

        var forecastTasks = providersToQuery.Select(provider =>
            FetchFromProviderAsync(provider, location, request.Date, cancellationToken));

        var forecastSources = await Task.WhenAll(forecastTasks);
        var aggregatedForecast = CalculateAggregatedForecast(forecastSources);

        var response = new WeatherForecastResponse
        {
            Location = location.ToLocationString(),
            Date = request.Date,
            AggregatedForecast = aggregatedForecast,
            Sources = forecastSources.Select(s => s.ToDto()).ToList()
        };

        await _cacheRepository.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15), cancellationToken);

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
        var enabledProviders = _weatherProviders.Where(p => p.IsEnabled);

        if (requestedSources == null || requestedSources.Count == 0)
        {
            _logger.LogDebug("No source filtering applied. Using all enabled providers");
            return enabledProviders;
        }

        var filteredProviders = enabledProviders
            .Where(p => requestedSources.Contains(p.SourceName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        _logger.LogDebug(
            "Filtered providers: {FilteredCount} out of {TotalCount} enabled providers",
            filteredProviders.Count,
            enabledProviders.Count());

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
            _logger.LogError(ex, "Error fetching forecast from {SourceName}", provider.SourceName);
            
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
