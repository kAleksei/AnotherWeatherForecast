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
    public IReadOnlyCollection<ForecastSource> Sources => _sources.AsReadOnly();

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
}
