using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Infrastructure.Configuration;
using WeatherForecast.Infrastructure.Responses;

namespace WeatherForecast.Infrastructure.ExternalServices.OpenWeatherMap;

/// <summary>
/// Handles historical weather data from OpenWeatherMap (past dates).
/// </summary>
public class OpenWeatherMapHistoricalProvider : IOpenWeatherMapHistoricalProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenWeatherMapHistoricalProvider> _logger;
    private readonly WeatherSourceOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ICoordinatesProvider _coordinatesProvider;

    public OpenWeatherMapHistoricalProvider(
        HttpClient httpClient,
        WeatherSourceOptions options,
        ResiliencePipeline resiliencePipeline,
        ICoordinatesProvider coordinatesProvider,
        ILogger<OpenWeatherMapHistoricalProvider> logger)
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

    public async Task<ForecastSource> GetHistoricalForecastAsync(
        Location location,
        DateTime date,
        string sourceName,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching historical weather from {SourceName} for {Location} on {Date}",
            sourceName, location, date.ToString("yyyy-MM-dd"));

        var (latitude, longitude) = await _coordinatesProvider.GetCoordinatesAsync(location, cancellationToken);
        var startUnix = new DateTimeOffset(date.Date).ToUnixTimeSeconds();
        var endUnix = new DateTimeOffset(date.Date.AddDays(1).AddSeconds(-1)).ToUnixTimeSeconds();
        var url = $"/data/2.5/history/city?lat={latitude}&lon={longitude}&type=hour&start={startUnix}&end={endUnix}&appid={_options.ApiKey}";

        var response = await _resiliencePipeline.ExecuteAsync(async ct =>
            await _httpClient.GetAsync(url, ct), cancellationToken);

        response.EnsureSuccessStatusCode();

        var apiResponse = await JsonSerializer.DeserializeAsync<OpenWeatherMapHistoryResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        if (apiResponse == null || apiResponse.List.Count == 0)
        {
            throw new InvalidOperationException($"No hourly historical data available for {date:yyyy-MM-dd}");
        }

        var avgTemp = apiResponse.List.Average(x => x.Main.Temp);
        var avgHumidity = apiResponse.List.Average(x => x.Main.Humidity);

        _logger.LogInformation("{SourceName} returned hourly historical avg temperature: {Temp}°C, avg humidity: {Humidity}%",
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
