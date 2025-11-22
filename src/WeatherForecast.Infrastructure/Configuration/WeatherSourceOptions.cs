namespace WeatherForecast.Infrastructure.Configuration;

/// <summary>
/// Configuration options for weather source providers.
/// </summary>
public class WeatherSourceOptions
{
    /// <summary>
    /// Gets or sets the base URL for the weather API.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key (optional, for providers that require authentication).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the timeout in seconds for HTTP requests.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the cache duration in minutes for this provider.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the historical data base URL (optional, for providers with separate historical endpoints).
    /// </summary>
    public string? HistoryBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the forecast data base URL (optional, for providers with separate forecast endpoints).
    /// </summary>
    public string? ForecastBaseUrl { get; set; }
}
