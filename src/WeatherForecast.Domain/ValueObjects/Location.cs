using System.Text.RegularExpressions;

namespace WeatherForecast.Domain.ValueObjects;

/// <summary>
/// Represents a geographic location with city and country.
/// </summary>
public sealed record Location : IEquatable<Location>
{
    private static readonly Regex CountryCodeRegex = new(@"^[A-Z]{2}$", RegexOptions.Compiled);

    /// <summary>
    /// Gets the city name.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Location"/> class.
    /// </summary>
    /// <param name="city">The city name.</param>
    /// <param name="country">The country code (ISO 3166-1 alpha-2).</param>
    /// <exception cref="ArgumentException">Thrown when city is empty or country code is invalid.</exception>
    public Location(string city, string country)
    {
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(country);

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City cannot be empty or whitespace.", nameof(city));
        }

        if (!CountryCodeRegex.IsMatch(country))
        {
            throw new ArgumentException(
                "Country must be a 2-letter uppercase ISO 3166-1 alpha-2 code (e.g., 'US', 'GB').",
                nameof(country));
        }

        City = city.Trim();
        Country = country;
    }

    /// <summary>
    /// Returns a string representation of the location.
    /// </summary>
    /// <returns>A string in the format "City, Country".</returns>
    public override string ToString() => $"{City}, {Country}";
}
