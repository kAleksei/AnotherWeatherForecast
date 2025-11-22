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
/// Weather source provider for OpenWeatherMap (requires API key).
/// </summary>
public class OpenWeatherMapProvider : IWeatherSourceProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenWeatherMapProvider> _logger;
    private readonly WeatherSourceOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;

    public const string SourceProviderName = "OpenWeatherMap";
    public string SourceName => SourceProviderName;

    public OpenWeatherMapProvider(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<WeatherSourceOptions> options,
        [FromKeyedServices(SourceProviderName)] ResiliencePipeline resiliencePipeline,
        ILogger<OpenWeatherMapProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resiliencePipeline);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClientFactory.CreateClient(nameof(OpenWeatherMapProvider));
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

            var daysDiff = (date.Date - DateTime.UtcNow.Date).Days;
            
            if (daysDiff <= 0)
            {
                var timestamp = new DateTimeOffset(date).ToUnixTimeSeconds();
                var url = $"/data/3.0/onecall/timemachine?lat={location.City}&lon={location.Country}&dt={timestamp}&appid={_options.ApiKey}&units=metric";
                
                var response = await _resiliencePipeline.ExecuteAsync(async ct => 
                    await _httpClient.GetAsync(url, ct), cancellationToken);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var data = JsonDocument.Parse(content);

                var root = data.RootElement;
                var current = root.GetProperty("data").EnumerateArray().FirstOrDefault();
                
                var tempValue = current.GetProperty("temp").GetDecimal();
                var humidityValue = current.GetProperty("humidity").GetDecimal();

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
            else
            {
                var url = $"/data/2.5/forecast?q={Uri.EscapeDataString(location.City)},{location.Country}&appid={_options.ApiKey}&units=metric";
                
                var response = await _resiliencePipeline.ExecuteAsync(async ct => 
                    await _httpClient.GetAsync(url, ct), cancellationToken);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var data = JsonDocument.Parse(content);

                var root = data.RootElement;
                var list = root.GetProperty("list");
                
                var targetDate = date.Date;
                var temperatures = new List<decimal>();
                var humidities = new List<decimal>();

                foreach (var item in list.EnumerateArray())
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()).DateTime;
                    if (dt.Date == targetDate)
                    {
                        var main = item.GetProperty("main");
                        temperatures.Add(main.GetProperty("temp").GetDecimal());
                        humidities.Add(main.GetProperty("humidity").GetDecimal());
                    }
                }

                if (temperatures.Count == 0)
                {
                    throw new InvalidOperationException($"No forecast data available for {date:yyyy-MM-dd}");
                }

                var avgTemp = temperatures.Average();
                var avgHumidity = humidities.Average();

                _logger.LogInformation("{SourceName} returned temperature: {Temp}°C, humidity: {Humidity}%", 
                    SourceName, avgTemp, avgHumidity);

                return new ForecastSource(
                    SourceName,
                    new Temperature(avgTemp),
                    new Humidity(avgHumidity),
                    true,
                    null,
                    DateTime.UtcNow);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutRejectedException or JsonException or InvalidOperationException or BrokenCircuitException)
        {
            _logger.LogError(ex, "Error fetching weather from {SourceName}", SourceName);
            return new ForecastSource(SourceName, null, null, false, ex.Message, DateTime.UtcNow);
        }
    }
}
