using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Domain.Entities;

/// <summary>
/// Represents a weather forecast aggregate root combining data from multiple sources.
/// </summary>
public class WeatherForecast
{
    private readonly List<ForecastSource> _sources = new();

    /// <summary>
    /// Gets the location for this forecast.
    /// </summary>
    public Location Location { get; }

    /// <summary>
    /// Gets the date for this forecast.
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// Gets the collection of forecast sources.
    /// </summary>
    public IReadOnlyList<ForecastSource> Sources => _sources.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherForecast"/> class.
    /// </summary>
    /// <param name="location">The location for this forecast.</param>
    /// <param name="date">The date for this forecast.</param>
    /// <exception cref="ArgumentNullException">Thrown when location is null.</exception>
    public WeatherForecast(Location location, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(location);

        Location = location;
        Date = date.Date;
    }

    /// <summary>
    /// Adds a forecast source to the collection.
    /// </summary>
    /// <param name="source">The forecast source to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    public void AddSource(ForecastSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _sources.Add(source);
    }

    /// <summary>
    /// Calculates the average temperature and humidity from all available sources.
    /// </summary>
    /// <returns>A tuple containing the average temperature and humidity, or null values if no data is available.</returns>
    public (Temperature? AverageTemperature, Humidity? AverageHumidity) CalculateAverage()
    {
        var availableSources = _sources.Where(s => s.Available).ToList();

        if (availableSources.Count == 0)
        {
            return (null, null);
        }

        var temperaturesWithData = availableSources
            .Where(s => s.Temperature != null)
            .Select(s => s.Temperature!)
            .ToList();

        var humiditiesWithData = availableSources
            .Where(s => s.Humidity != null)
            .Select(s => s.Humidity!)
            .ToList();

        Temperature? avgTemperature = null;
        if (temperaturesWithData.Count > 0)
        {
            var avgCelsius = temperaturesWithData.Average(t => t.Celsius);
            avgTemperature = new Temperature(avgCelsius);
        }

        Humidity? avgHumidity = null;
        if (humiditiesWithData.Count > 0)
        {
            var avgPercent = humiditiesWithData.Average(h => h.Percent);
            avgHumidity = new Humidity(avgPercent);
        }

        return (avgTemperature, avgHumidity);
    }
}
