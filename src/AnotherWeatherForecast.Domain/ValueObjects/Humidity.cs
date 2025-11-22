namespace AnotherWeatherForecast.Domain.ValueObjects;

/// <summary>
/// Represents a humidity percentage value.
/// </summary>
public sealed record Humidity : IEquatable<Humidity>
{
    private const decimal MinPercent = 0m;
    private const decimal MaxPercent = 100m;

    /// <summary>
    /// Gets the humidity percentage.
    /// </summary>
    public decimal Percent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Humidity"/> class.
    /// </summary>
    /// <param name="percent">The humidity percentage.</param>
    /// <exception cref="ArgumentException">Thrown when the humidity is outside the valid range (0-100%).</exception>
    public Humidity(decimal percent)
    {
        if (percent < MinPercent || percent > MaxPercent)
        {
            throw new ArgumentException(
                $"Humidity must be between {MinPercent}% and {MaxPercent}%. Provided: {percent}%",
                nameof(percent));
        }

        Percent = percent;
    }

    /// <summary>
    /// Implicitly converts a decimal value to a <see cref="Humidity"/>.
    /// </summary>
    /// <param name="percent">The humidity percentage.</param>
    public static implicit operator Humidity(decimal percent) => new(percent);

    /// <summary>
    /// Explicitly converts a <see cref="Humidity"/> to a decimal value.
    /// </summary>
    /// <param name="humidity">The humidity instance.</param>
    public static explicit operator decimal(Humidity humidity) => humidity.Percent;

    /// <summary>
    /// Returns a string representation of the humidity.
    /// </summary>
    /// <returns>A string representation of the humidity.</returns>
    public override string ToString() => $"{Percent}%";
}
