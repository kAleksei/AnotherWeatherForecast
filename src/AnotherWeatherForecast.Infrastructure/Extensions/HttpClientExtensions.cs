using AnotherWeatherForecast.Infrastructure.Configuration;
using AnotherWeatherForecast.Infrastructure.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnotherWeatherForecast.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering HTTP clients for weather source providers.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Registers all HTTP clients for weather source providers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWeatherHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register OpenMeteo HTTP client
        services.AddHttpClient(OpenMeteoProvider.SourceProviderName, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<WeatherSourceOptions>>()
                .Get(OpenMeteoProvider.SourceProviderName);
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds + 1);
        });

        // Register WeatherAPI HTTP client
        services.AddHttpClient(WeatherApiProvider.SourceProviderName, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<WeatherSourceOptions>>()
                .Get(WeatherApiProvider.SourceProviderName);
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds + 1);
        });

        // Register OpenWeatherMap Historical HTTP client
        services.AddHttpClient("OpenWeatherMap.Historical", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<WeatherSourceOptions>>()
                .Get(OpenWeatherMapProvider.SourceProviderName);
            var historyBaseUrl = options.HistoryBaseUrl ?? throw new InvalidOperationException(
                $"{OpenWeatherMapProvider.SourceProviderName} HistoryBaseUrl is not configured");
            client.BaseAddress = new Uri(historyBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds + 1);
        });

        // Register OpenWeatherMap Forecast HTTP client
        services.AddHttpClient("OpenWeatherMap.Forecast", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<WeatherSourceOptions>>()
                .Get(OpenWeatherMapProvider.SourceProviderName);
            var forecastBaseUrl = options.ForecastBaseUrl ?? options.BaseUrl;
            client.BaseAddress = new Uri(forecastBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds + 1);
        });

        return services;
    }
}
