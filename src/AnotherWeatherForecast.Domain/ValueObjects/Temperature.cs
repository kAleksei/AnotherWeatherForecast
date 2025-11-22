namespace AnotherWeatherForecast.Domain.ValueObjects;

/// <summary>
/// Represents a temperature value in Celsius.
/// </summary>
public sealed record Temperature : IEquatable<Temperature>
{
    private const decimal MinCelsius = -100m;
    private const decimal MaxCelsius = 60m;

    /// <summary>
    /// Gets the temperature in Celsius.
    /// </summary>
    public decimal Celsius { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Temperature"/> class.
    /// </summary>
    /// <param name="celsius">The temperature in Celsius.</param>
    /// <exception cref="ArgumentException">Thrown when the temperature is outside the valid range (-100 to 60°C).</exception>
    public Temperature(decimal celsius)
    {
        if (celsius < MinCelsius || celsius > MaxCelsius)
        {
            throw new ArgumentException(
                $"Temperature must be between {MinCelsius}°C and {MaxCelsius}°C. Provided: {celsius}°C",
                nameof(celsius));
        }

        Celsius = celsius;
    }

    /// <summary>
    /// Gets the temperature in Fahrenheit.
    /// </summary>
    public decimal Fahrenheit => (Celsius * 9m / 5m) + 32m;

    /// <summary>
    /// Implicitly converts a decimal value to a <see cref="Temperature"/>.
    /// </summary>
    /// <param name="celsius">The temperature in Celsius.</param>
    public static implicit operator Temperature(decimal celsius) => new(celsius);

    /// <summary>
    /// Explicitly converts a <see cref="Temperature"/> to a decimal value.
    /// </summary>
    /// <param name="temperature">The temperature instance.</param>
    public static explicit operator decimal(Temperature temperature) => temperature.Celsius;

    /// <summary>
    /// Returns a string representation of the temperature.
    /// </summary>
    /// <returns>A string representation of the temperature.</returns>
    public override string ToString() => $"{Celsius}°C";
}
