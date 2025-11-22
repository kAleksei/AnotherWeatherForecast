using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Domain.Tests;

public class LocationTests
{
    [Fact]
    public void Constructor_WithValidCityAndCountry_CreatesLocation()
    {
        // Arrange & Act
        var location = new Location("New York", "US");

        // Assert
        Assert.Equal("New York", location.City);
        Assert.Equal("US", location.Country);
    }

    [Fact]
    public void Constructor_WithCityWithWhitespace_TrimsCity()
    {
        // Arrange & Act
        var location = new Location("  London  ", "GB");

        // Assert
        Assert.Equal("London", location.City);
        Assert.Equal("GB", location.Country);
    }

    [Fact]
    public void Constructor_WithNullCity_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Location(null!, "US"));
    }

    [Fact]
    public void Constructor_WithEmptyCity_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Location("", "US"));
        Assert.Contains("City cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithWhitespaceCity_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Location("   ", "US"));
        Assert.Contains("City cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullCountry_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Location("Paris", null!));
    }

    [Fact]
    public void Constructor_WithInvalidCountryCode_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Location("Paris", "France"));
        Assert.Contains("Country must be a 2-letter uppercase", exception.Message);
    }

    [Fact]
    public void Constructor_WithLowercaseCountryCode_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Location("Paris", "fr"));
        Assert.Contains("Country must be a 2-letter uppercase", exception.Message);
    }

    [Fact]
    public void Constructor_WithSingleLetterCountryCode_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Location("Paris", "F"));
        Assert.Contains("Country must be a 2-letter uppercase", exception.Message);
    }

    [Fact]
    public void Constructor_WithThreeLetterCountryCode_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Location("Paris", "FRA"));
        Assert.Contains("Country must be a 2-letter uppercase", exception.Message);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("GB")]
    [InlineData("FR")]
    [InlineData("DE")]
    [InlineData("JP")]
    public void Constructor_WithValidCountryCodes_CreatesLocation(string countryCode)
    {
        // Arrange & Act
        var location = new Location("City", countryCode);

        // Assert
        Assert.Equal(countryCode, location.Country);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var location1 = new Location("Paris", "FR");
        var location2 = new Location("Paris", "FR");

        // Act & Assert
        Assert.Equal(location1, location2);
        Assert.True(location1 == location2);
    }

    [Fact]
    public void Equals_WithDifferentCity_ReturnsFalse()
    {
        // Arrange
        var location1 = new Location("Paris", "FR");
        var location2 = new Location("Lyon", "FR");

        // Act & Assert
        Assert.NotEqual(location1, location2);
        Assert.True(location1 != location2);
    }

    [Fact]
    public void Equals_WithDifferentCountry_ReturnsFalse()
    {
        // Arrange
        var location1 = new Location("London", "GB");
        var location2 = new Location("London", "US");

        // Act & Assert
        Assert.NotEqual(location1, location2);
        Assert.True(location1 != location2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        // Arrange
        var location1 = new Location("Tokyo", "JP");
        var location2 = new Location("Tokyo", "JP");

        // Act & Assert
        Assert.Equal(location1.GetHashCode(), location2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var location = new Location("Berlin", "DE");

        // Act
        var result = location.ToString();

        // Assert
        Assert.Equal("Berlin, DE", result);
    }
}
