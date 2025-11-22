using System.Reflection;
using AnotherWeatherForecast.Application.Common.Interfaces;
using AnotherWeatherForecast.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AnotherWeatherForecast.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer services, validators, and MediatR behaviors.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register application services
        services.AddScoped<IWeatherAggregationService, WeatherAggregationService>();

        return services;
    }
}
