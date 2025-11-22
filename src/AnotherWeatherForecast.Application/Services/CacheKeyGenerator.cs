namespace AnotherWeatherForecast.Application.Services;

/// <summary>
/// Generates cache keys for weather forecast data.
/// </summary>
public static class CacheKeyGenerator
{
    /// <summary>
    /// Generates a cache key for weather forecast data.
    /// Format: weather:{city}:{country}:{yyyyMMdd}:{sources|all}
    /// </summary>
    /// <param name="city">The city name.</param>
    /// <param name="country">The country code.</param>
    /// <param name="date">The forecast date.</param>
    /// <param name="sources">Optional list of specific sources. If null or empty, uses "all".</param>
    /// <returns>A formatted cache key.</returns>
    public static string GenerateKey(string city, string country, DateTime date, List<string>? sources)
    {
        var sourcesKey = sources == null || sources.Count == 0
            ? "all"
            : string.Join("-", sources.OrderBy(s => s.ToLowerInvariant()));

        return $"weather:{city.ToLowerInvariant()}:{country.ToLowerInvariant()}:{date:yyyyMMdd}:{sourcesKey}";
    }
}
