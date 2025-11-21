using WeatherForecast.Domain.Enums;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Domain.Entities;

/// <summary>
/// Represents a weather forecast from a specific data source.
/// </summary>
public class ForecastSource
{
    /// <summary>
    /// Gets the name of the weather data source.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of the weather data source.
    /// </summary>
    public WeatherSourceType SourceType { get; }

    /// <summary>
    /// Gets the temperature from this source, if available.
    /// </summary>
    public Temperature? Temperature { get; }

    /// <summary>
    /// Gets the humidity from this source, if available.
    /// </summary>
    public Humidity? Humidity { get; }

    /// <summary>
    /// Gets a value indicating whether this source is available and returned valid data.
    /// </summary>
    public bool Available { get; }

    /// <summary>
    /// Gets the error message if the source was not available.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the timestamp when the data was retrieved.
    /// </summary>
    public DateTime RetrievedAt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForecastSource"/> class.
    /// </summary>
    /// <param name="name">The name of the weather data source.</param>
    /// <param name="sourceType">The type of the weather data source.</param>
    /// <param name="temperature">The temperature from this source.</param>
    /// <param name="humidity">The humidity from this source.</param>
    /// <param name="available">Whether this source is available.</param>
    /// <param name="error">The error message if the source was not available.</param>
    /// <param name="retrievedAt">The timestamp when the data was retrieved.</param>
    /// <exception cref="ArgumentException">Thrown when name is empty.</exception>
    public ForecastSource(
        string name,
        WeatherSourceType sourceType,
        Temperature? temperature,
        Humidity? humidity,
        bool available,
        string? error,
        DateTime retrievedAt)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
        }

        Name = name.Trim();
        SourceType = sourceType;
        Temperature = temperature;
        Humidity = humidity;
        Available = available;
        Error = error;
        RetrievedAt = retrievedAt;
    }
}
