using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;
using AnotherWeatherForecast.Infrastructure.Configuration;
using AnotherWeatherForecast.Infrastructure.Responses;

namespace AnotherWeatherForecast.Infrastructure.ExternalServices.OpenWeatherMap;

/// <summary>
/// Handles future weather forecast data from OpenWeatherMap (future dates up to 5 days).
/// Uses the 5-day/3-hour forecast API.
/// </summary>
public class OpenWeatherMapForecastProvider : IOpenWeatherMapForecastProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenWeatherMapForecastProvider> _logger;
    private readonly WeatherSourceOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ICoordinatesProvider _coordinatesProvider;

    public OpenWeatherMapForecastProvider(
        HttpClient httpClient,
        WeatherSourceOptions options,
        ResiliencePipeline resiliencePipeline,
        ICoordinatesProvider coordinatesProvider,
        ILogger<OpenWeatherMapForecastProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resiliencePipeline);
        ArgumentNullException.ThrowIfNull(coordinatesProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _options = options;
        _resiliencePipeline = resiliencePipeline;
        _coordinatesProvider = coordinatesProvider;
        _logger = logger;
    }

    public async Task<ForecastSource> GetDailyForecastAsync(
        Location location,
        DateTime date,
        string sourceName,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching daily forecast from {SourceName} for {Location} on {Date}",
            sourceName, location, date.ToString("yyyy-MM-dd"));

        var (latitude, longitude) = await _coordinatesProvider.GetCoordinatesAsync(location, cancellationToken);
        var url = $"/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&appid={_options.ApiKey}";

        var response = await _resiliencePipeline.ExecuteAsync(async ct =>
            await _httpClient.GetAsync(url, ct), cancellationToken);

        response.EnsureSuccessStatusCode();

        var forecastResponse = await JsonSerializer.DeserializeAsync<OpenWeatherMapForecastResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        if (forecastResponse == null || forecastResponse.List.Count == 0)
        {
            throw new InvalidOperationException($"No forecast data available for {date:yyyy-MM-dd}");
        }

        // Filter items for the target date and calculate averages
        var targetDate = date.Date;
        var itemsForDate = forecastResponse.List
            .Where(item => DateTimeOffset.FromUnixTimeSeconds(item.Dt).Date == targetDate)
            .ToList();

        if (itemsForDate.Count == 0)
        {
            throw new InvalidOperationException($"No forecast data available for {date:yyyy-MM-dd}");
        }

        var avgTemp = itemsForDate.Average(item => item.Main.Temp);
        var avgHumidity = itemsForDate.Average(item => item.Main.Humidity);

        _logger.LogInformation("{SourceName} returned daily forecast avg temperature: {Temp}°C, avg humidity: {Humidity}%",
            sourceName, avgTemp, avgHumidity);

        return new ForecastSource(
            sourceName,
            new Temperature(avgTemp),
            new Humidity(avgHumidity),
            true,
            null,
            DateTime.UtcNow);
    }
}
