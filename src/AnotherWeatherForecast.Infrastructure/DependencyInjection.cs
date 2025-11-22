using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using AnotherWeatherForecast.Application.Common.Interfaces;
using AnotherWeatherForecast.Infrastructure.Caching;
using AnotherWeatherForecast.Infrastructure.Configuration;
using AnotherWeatherForecast.Infrastructure.ExternalServices;
using AnotherWeatherForecast.Infrastructure.ExternalServices.OpenWeatherMap;
using AnotherWeatherForecast.Infrastructure.Extensions;

namespace AnotherWeatherForecast.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services, HTTP clients, resilience policies, and caching.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddWeatherHttpClients(configuration);
        
        services.AddSingleton<ICoordinatesProvider, CoordinatesProvider>();
        
        // Register OpenWeatherMap nested providers as keyed services
        RegisterNestedProvider<IOpenWeatherMapHistoricalProvider, OpenWeatherMapHistoricalProvider>(
            services, "OpenWeatherMap.Historical", OpenWeatherMapProvider.SourceProviderName);
        RegisterNestedProvider<IOpenWeatherMapForecastProvider, OpenWeatherMapForecastProvider>(
            services, "OpenWeatherMap.Forecast", OpenWeatherMapProvider.SourceProviderName);
        
        RegisterSourceProvider<OpenMeteoProvider>(services, configuration, OpenMeteoProvider.SourceProviderName);
        RegisterSourceProvider<OpenWeatherMapProvider>(services, configuration, OpenWeatherMapProvider.SourceProviderName);
        RegisterSourceProvider<WeatherApiProvider>(services, configuration, WeatherApiProvider.SourceProviderName);
        
        return services;
    }

    private static void RegisterNestedProvider<TInterface, TImplementation>(
        IServiceCollection services,
        string httpClientName,
        string sourceName)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddKeyedSingleton<TInterface>(httpClientName, (serviceProvider, key) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(httpClientName);
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<WeatherSourceOptions>>();
            var options = optionsMonitor.Get(sourceName);
            var resiliencePipeline = serviceProvider.GetRequiredKeyedService<ResiliencePipeline>(sourceName);
            var coordinatesProvider = serviceProvider.GetRequiredService<ICoordinatesProvider>();
            var logger = serviceProvider.GetRequiredService<ILogger<TImplementation>>();
            
            return (TInterface)Activator.CreateInstance(
                typeof(TImplementation),
                httpClient,
                options,
                resiliencePipeline,
                coordinatesProvider,
                logger)!;
        });
    }

    private static void RegisterSourceProvider<TSourceProvider>(
        IServiceCollection services, 
        IConfiguration configuration,
        string sourceName)
        where TSourceProvider : class, IWeatherSourceProvider
    {
        services.Configure<WeatherSourceOptions>(
            sourceName,
            configuration.GetSection($"WeatherSources:{sourceName}"));
        services.DecorateWithCache<TSourceProvider>();
        
        // Register keyed resilience pipeline factory
        services.AddKeyedSingleton<ResiliencePipeline>(sourceName, (serviceProvider, key) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ResiliencePipeline>>();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<WeatherSourceOptions>>();
            var options = optionsMonitor.Get(sourceName);
            
            return CreateResiliencePipeline(sourceName, options.TimeoutSeconds, logger);
        });
    }

    private static IServiceCollection DecorateWithCache<TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, IWeatherSourceProvider
    {
        services.AddSingleton<TImplementation>();
        
        return services.AddSingleton<IWeatherSourceProvider, CachedWeatherSourceProvider>(provider =>
        {
            var innerProvider = provider.GetRequiredService<TImplementation>();
            var cache = provider.GetRequiredService<IMemoryCache>();
            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<WeatherSourceOptions>>();
            var options = optionsMonitor.Get(innerProvider.SourceName);
            var logger = provider.GetRequiredService<ILogger<CachedWeatherSourceProvider>>();

            return new CachedWeatherSourceProvider(innerProvider, cache, options, logger);
        });
    }

    /// <summary>
    /// Creates a resilience pipeline for weather source providers with retry, circuit breaker, and timeout policies.
    /// </summary>
    /// <param name="sourceName">The name of the weather source for logging context.</param>
    /// <param name="timeoutSeconds">The timeout duration in seconds.</param>
    /// <param name="logger">The logger instance for resilience events.</param>
    /// <returns>A configured resilience pipeline.</returns>
    private static ResiliencePipeline CreateResiliencePipeline(
        string sourceName,
        int timeoutSeconds,
        ILogger logger)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning("Retry attempt {AttemptNumber} for {SourceName} after {Delay}ms", 
                        args.AttemptNumber, sourceName, args.RetryDelay.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                SamplingDuration = TimeSpan.FromSeconds(60),
                OnOpened = args =>
                {
                    logger.LogWarning("Circuit breaker opened for {SourceName} after {FailureCount} consecutive failures",
                        sourceName, 3);
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    logger.LogInformation("Circuit breaker half-opened for {SourceName}, attempting recovery",
                        sourceName);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker closed for {SourceName}, service restored",
                        sourceName);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(timeoutSeconds))
            .Build();
    }
}
