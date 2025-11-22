using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using AnotherWeatherForecast.Application.Common.Interfaces;
using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;
using AnotherWeatherForecast.Infrastructure.Configuration;

namespace AnotherWeatherForecast.Infrastructure.Caching;

/// <summary>
/// Decorator that adds caching functionality to weather source providers using HybridCache.
/// Implements cache-aside pattern with stale-while-revalidate fallback.
/// </summary>
public class CachedWeatherSourceProvider : IWeatherSourceProvider
{
    private readonly IWeatherSourceProvider _innerProvider;
    private readonly IMemoryCache _cache;
    private readonly WeatherSourceOptions _options;
    private readonly ILogger<CachedWeatherSourceProvider> _logger;

    private readonly TimeSpan _staleCacheDuration = TimeSpan.FromHours(24);
    public string SourceName => _innerProvider.SourceName;

    public CachedWeatherSourceProvider(
        IWeatherSourceProvider innerProvider,
        IMemoryCache cache,
        WeatherSourceOptions options,
        ILogger<CachedWeatherSourceProvider> logger)
    {
        _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ForecastSource> GetForecastAsync(
        Location location,
        DateTime date,
        CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(location, date);
        var staleKey = GenerateStaleCacheKey(location, date);

        _logger.LogDebug("Attempting to fetch forecast for {SourceName}. Key: {CacheKey}", SourceName, cacheKey);

        try
        {
            // Try to get from cache or fetch fresh data
            var result = await _cache.GetOrCreateAsync<ForecastSource>(
                cacheKey,
                async entry =>
                {
                    var freshData = await _innerProvider.GetForecastAsync(location, date, cancellationToken);

                    // Store a copy with extended TTL for stale fallback
                    if (freshData.Available)
                    {
                        _cache.Set(
                            staleKey,
                            freshData,
                            _staleCacheDuration);
                    }

                    entry.Value = freshData;
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDurationMinutes);
                    return freshData;
                });

            _logger.LogDebug("Successfully fetched forecast for {SourceName}", SourceName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, 
                "Failed to fetch forecast from {SourceName}. Attempting to retrieve stale data from cache", 
                SourceName);

            // Try to get stale data from cache as fallback
            try
            {
                var staleData = await _cache.GetOrCreateAsync<ForecastSource?>(
                    staleKey, _ => Task.FromResult(null as ForecastSource));
                
                if (staleData != null)
                {
                    _logger.LogInformation(
                        "Returning stale cached data for {SourceName} due to source failure", 
                        SourceName);
                    return staleData;
                }
            }
            catch (Exception cacheEx)
            {
                _logger.LogError(cacheEx, 
                    "Failed to retrieve stale data from cache for {SourceName}", 
                    SourceName);
            }

            // No cache available, return unavailable forecast
            _logger.LogError(ex, 
                "No cached data available for {SourceName}. Returning unavailable forecast", 
                SourceName);
            
            return new ForecastSource(
                sourceName: SourceName,
                temperature: null,
                humidity: null,
                available: false,
                error: $"Failed to fetch forecast: {ex.Message}",
                retrievedAt: DateTime.UtcNow);
        }
    }

    private string GenerateCacheKey(Location location, DateTime date)
    {
        return $"weather:{SourceName}:{location.City}:{location.Country}:{date:yyyyMMdd}";
    }

    private string GenerateStaleCacheKey(Location location, DateTime date)
    {
        return $"weather:stale:{SourceName}:{location.City}:{location.Country}:{date:yyyyMMdd}";
    }
}
