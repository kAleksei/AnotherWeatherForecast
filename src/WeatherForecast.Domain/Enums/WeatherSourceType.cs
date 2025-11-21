namespace WeatherForecast.Domain.Enums;

/// <summary>
/// Represents the type of weather data source.
/// </summary>
public enum WeatherSourceType
{
    /// <summary>
    /// Open-Meteo weather service.
    /// </summary>
    OpenMeteo,

    /// <summary>
    /// WeatherAPI service.
    /// </summary>
    WeatherAPI,

    /// <summary>
    /// OpenWeatherMap service.
    /// </summary>
    OpenWeatherMap
}
