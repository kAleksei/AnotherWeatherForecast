using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using AnotherWeatherForecast.Application.Common.Interfaces;
using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;
using AnotherWeatherForecast.Infrastructure.Configuration;

namespace AnotherWeatherForecast.Infrastructure.ExternalServices;

/// <summary>
/// Weather source provider for WeatherAPI.com (requires API key).
/// </summary>
public class WeatherApiProvider : IWeatherSourceProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherApiProvider> _logger;
    private readonly WeatherSourceOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ICoordinatesProvider _coordinatesProvider;

    public const string SourceProviderName = "WeatherAPI";
    public string SourceName => SourceProviderName;

    public WeatherApiProvider(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<WeatherSourceOptions> options,
        [FromKeyedServices(SourceProviderName)] ResiliencePipeline resiliencePipeline,
        ICoordinatesProvider coordinatesProvider,
        ILogger<WeatherApiProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resiliencePipeline);
        ArgumentNullException.ThrowIfNull(coordinatesProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClientFactory.CreateClient(nameof(WeatherApiProvider));
        _options = options.Get(SourceProviderName);
        _resiliencePipeline = resiliencePipeline;
        _coordinatesProvider = coordinatesProvider;
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

            var daysDiff = (date.Date - DateTime.UtcNow.Date).Days;
            var endpoint = daysDiff <= 0 ? "history" : "forecast";
            
            var (latitude, longitude) = await _coordinatesProvider.GetCoordinatesAsync(location, cancellationToken);
            var query = $"{latitude},{longitude}";
            var url = $"/v1/{endpoint}.json?key={_options.ApiKey}&q={Uri.EscapeDataString(query)}&dt={date:yyyy-MM-dd}";

            var response = await _resiliencePipeline.ExecuteAsync(async ct => 
                await _httpClient.GetAsync(url, ct), cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonDocument.Parse(content);

            var root = data.RootElement;
            var forecastData = root.GetProperty("forecast");
            var forecastDay = forecastData.GetProperty("forecastday").EnumerateArray().FirstOrDefault();
            var day = forecastDay.GetProperty("day");

            var tempValue = day.GetProperty("avgtemp_c").GetDecimal();
            var humidityValue = day.GetProperty("avghumidity").GetDecimal();

            _logger.LogInformation("{SourceName} returned temperature: {Temp}°C, humidity: {Humidity}%", 
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
}
