using AnotherWeatherForecast.Application.Common.Models;
using AnotherWeatherForecast.Domain.Entities;
using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Application.Common.Extensions;

/// <summary>
/// Extension methods for mapping domain entities to DTOs.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Converts a ForecastSource entity to ForecastSourceDto.
    /// </summary>
    public static ForecastSourceDto ToDto(this ForecastSource source)
    {
        return new ForecastSourceDto
        {
            SourceName = source.SourceName,
            TemperatureCelsius = source.Temperature?.Celsius,
            HumidityPercent = source.Humidity?.Percent,
            Available = source.Available,
            ProblemDetails = source.Error != null 
                ? new ProblemDetails 
                { 
                    Title = "Source Unavailable", 
                    Detail = source.Error,
                } 
                : null,
            RetrievedAt = source.RetrievedAt
        };
    }

    /// <summary>
    /// Converts a collection of ForecastSource entities to DTOs.
    /// </summary>
    public static List<ForecastSourceDto> ToDto(this IEnumerable<ForecastSource> sources)
    {
        return sources.Select(s => s.ToDto()).ToList();
    }

    /// <summary>
    /// Converts a Location value object to a string representation.
    /// </summary>
    public static string ToLocationString(this Location location)
    {
        return location.ToString();
    }
}
