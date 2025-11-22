using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Application.Common.Interfaces;

namespace WeatherForecast.Infrastructure.ExternalServices;

public interface ICoordinatesProvider
{
    Task<(decimal latitude, decimal longitude)> GetCoordinatesAsync(Location location, CancellationToken cancellationToken);
}

public class CoordinatesProvider : ICoordinatesProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoordinatesProvider> _logger;

    public CoordinatesProvider(IHttpClientFactory httpClientFactory, ILogger<CoordinatesProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient("CoordinatesProvider");
        _logger = logger;
    }

    public async Task<(decimal latitude, decimal longitude)> GetCoordinatesAsync(Location location, CancellationToken cancellationToken)
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
