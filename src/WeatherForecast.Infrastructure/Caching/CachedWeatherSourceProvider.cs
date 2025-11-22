using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Infrastructure.Configuration;

namespace WeatherForecast.Infrastructure.Caching;

/// <summary>
/// Decorator that adds caching functionality to weather source providers using HybridCache.
/// Implements cache-aside pattern with stale-while-revalidate fallback.
/// </summary>
public class CachedWeatherSourceProvider : IWeatherSourceProvider
{
    private readonly IWeatherSourceProvider _innerProvider;
    private readonly HybridCache _cache;
    private readonly WeatherSourceOptions _options;
    private readonly ILogger<CachedWeatherSourceProvider> _logger;

    private readonly TimeSpan _staleCacheDuration = TimeSpan.FromHours(24);
    public string SourceName => _innerProvider.SourceName;

    public CachedWeatherSourceProvider(
        IWeatherSourceProvider innerProvider,
        HybridCache cache,
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
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                async cancel =>
                {
                    var freshData = await _innerProvider.GetForecastAsync(location, date, cancel);
                    
                    // Store a copy with extended TTL for stale fallback
                    if (freshData.Available)
                    {
                        await _cache.SetAsync(
                            staleKey,
                            freshData,
                            new HybridCacheEntryOptions
                            {
                                // Keep stale data longer
                                Expiration = _staleCacheDuration,
                                LocalCacheExpiration = _staleCacheDuration,
                            },
                            cancellationToken: cancel);
                    }
                    
                    return freshData;
                },
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(_options.CacheDurationMinutes),
                    LocalCacheExpiration = TimeSpan.FromMinutes(_options.CacheDurationMinutes)
                },
                cancellationToken: cancellationToken);

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
                    staleKey,
                    _ => ValueTask.FromResult<ForecastSource?>(null),
                    cancellationToken: cancellationToken);
                
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
