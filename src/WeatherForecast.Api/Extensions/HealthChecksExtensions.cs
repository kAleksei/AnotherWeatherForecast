using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WeatherForecast.Infrastructure.Configuration;
using WeatherForecast.Infrastructure.ExternalServices;

namespace WeatherForecast.Api.Extensions;

public static class HealthChecksExtensions
{
    public static void ConfigureHealthChecks(WebApplicationBuilder webApplicationBuilder)
    {
        var weatherSection = webApplicationBuilder.Configuration.GetRequiredSection("WeatherSources");
        var openMeteoOptions = weatherSection.GetRequiredSection(OpenMeteoProvider.SourceProviderName).Get<WeatherSourceOptions>();
        var openWeatherMapOptions = weatherSection.GetRequiredSection(OpenWeatherMapProvider.SourceProviderName).Get<WeatherSourceOptions>();
        var weatherApiOptions = weatherSection.GetRequiredSection(WeatherApiProvider.SourceProviderName).Get<WeatherSourceOptions>();
        
        var healthChecks = webApplicationBuilder.Services.AddHealthChecks()
            .AddUrlGroup(new Uri(openMeteoOptions!.BaseUrl), name: nameof(OpenMeteoProvider),
                failureStatus: HealthStatus.Degraded,
                tags: new[] {"health"});

        // Add OpenWeatherMap forecast health check
        var forecastBaseUrl = openWeatherMapOptions!.ForecastBaseUrl ?? openWeatherMapOptions.BaseUrl;
        healthChecks.AddUrlGroup(new Uri(forecastBaseUrl), name: $"{nameof(OpenWeatherMapProvider)}_Forecast",
            failureStatus: HealthStatus.Degraded,
            tags: new[] {"startup", "ready", "health"});

        // Add OpenWeatherMap historical health check if configured
        if (!string.IsNullOrEmpty(openWeatherMapOptions.HistoryBaseUrl))
        {
            healthChecks.AddUrlGroup(new Uri(openWeatherMapOptions.HistoryBaseUrl), name: $"{nameof(OpenWeatherMapProvider)}_History",
                failureStatus: HealthStatus.Degraded,
                tags: new[] {"startup", "ready", "health"});
        }

        healthChecks.AddUrlGroup(new Uri(weatherApiOptions!.BaseUrl), name: nameof(WeatherApiProvider),
            failureStatus: HealthStatus.Degraded,
            tags: new[] {"startup", "ready", "health"});
    }

    public static void MapHealthCheckEndpoints(this WebApplication app)
    {
        // Startup health check: all checks with "startup" tag
        app.MapHealthChecks("/health/startup", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("startup"),
            ResponseWriter = WriteHealthCheckResponse,
            AllowCachingResponses = true
        });

        // Readiness health check: all checks with "ready" tag
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse,
            AllowCachingResponses = true
        });

        // Liveness health check: always healthy if app is running
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponse,
            AllowCachingResponses = true
        });

        // General health check: all checks with "health" tag
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse,
            AllowCachingResponses = true
        });
    }

    private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString()
            })
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}