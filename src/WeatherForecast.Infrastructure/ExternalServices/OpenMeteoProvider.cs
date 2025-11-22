using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using WeatherForecast.Application.Common.Interfaces;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Infrastructure.Configuration;

namespace WeatherForecast.Infrastructure.ExternalServices;

/// <summary>
/// Weather source provider for Open-Meteo API (free, no API key required).
/// </summary>
public class OpenMeteoProvider : IWeatherSourceProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenMeteoProvider> _logger;
    private readonly WeatherSourceOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;

    public const string SourceProviderName = "OpenMeteo";
    public string SourceName => SourceProviderName;
    
    public OpenMeteoProvider(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<WeatherSourceOptions> options,
        [FromKeyedServices(SourceProviderName)] ResiliencePipeline resiliencePipeline,
        ILogger<OpenMeteoProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resiliencePipeline);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClientFactory.CreateClient(nameof(OpenMeteoProvider));
        _options = options.Get(SourceName);
        _resiliencePipeline = resiliencePipeline;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds + 1);
    }

    public async Task<ForecastSource> GetForecastAsync(
        Location location,
        DateTime date,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching weather from {SourceName} for {Location} on {Date}", 
                SourceName, location, date.ToString("yyyy-MM-dd"));

            var (latitude, longitude) = await GetCoordinatesAsync(location, cancellationToken);
            
            var url = $"/v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_mean,relative_humidity_2m_mean&timezone=auto&start_date={date:yyyy-MM-dd}&end_date={date:yyyy-MM-dd}";

            var response = await _resiliencePipeline.ExecuteAsync(async ct => 
                await _httpClient.GetAsync(url, ct), cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonDocument.Parse(content);

            var root = data.RootElement;
            var daily = root.GetProperty("daily");
            var temperatures = daily.GetProperty("temperature_2m_mean");
            var humidities = daily.GetProperty("relative_humidity_2m_mean");

            var tempValue = temperatures.EnumerateArray().FirstOrDefault().GetDecimal();
            var humidityValue = humidities.EnumerateArray().FirstOrDefault().GetDecimal();

            _logger.LogInformation("{SourceName} returned temperature: {Temp}C, humidity: {Humidity}%", 
                SourceName, tempValue, humidityValue);

            return new ForecastSource(
                SourceName,
                new Temperature(tempValue),
                new Humidity(humidityValue),
                true,
                null,
                DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or JsonException or BrokenCircuitException)
        {
            _logger.LogError(ex, "Error fetching weather from {SourceName}", SourceName);
            return new ForecastSource(SourceName, null, null, false, ex.Message, DateTime.UtcNow);
        }
    }

    private async Task<(decimal latitude, decimal longitude)> GetCoordinatesAsync(
        Location location,
        CancellationToken cancellationToken)
    {
        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location.City)}&count=1&language=en&format=json";
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonDocument.Parse(content);
        
        var results = data.RootElement.GetProperty("results");
        var firstResult = results.EnumerateArray().FirstOrDefault();

        var latitude = firstResult.GetProperty("latitude").GetDecimal();
        var longitude = firstResult.GetProperty("longitude").GetDecimal();

        return (latitude, longitude);
    }
}
