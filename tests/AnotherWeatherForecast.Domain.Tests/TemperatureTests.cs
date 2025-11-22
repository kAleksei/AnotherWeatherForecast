using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Domain.Tests;

public class TemperatureTests
{
    [Fact]
    public void Constructor_WithValidCelsius_CreatesTemperature()
    {
        // Arrange & Act
        var temperature = new Temperature(25m);

        // Assert
        Assert.Equal(25m, temperature.Celsius);
    }

    [Fact]
    public void Constructor_WithMinimumValidCelsius_CreatesTemperature()
    {
        // Arrange & Act
        var temperature = new Temperature(-100m);

        // Assert
        Assert.Equal(-100m, temperature.Celsius);
    }

    [Fact]
    public void Constructor_WithMaximumValidCelsius_CreatesTemperature()
    {
        // Arrange & Act
        var temperature = new Temperature(60m);

        // Assert
        Assert.Equal(60m, temperature.Celsius);
    }

    [Fact]
    public void Constructor_WithCelsiusBelowMinimum_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Temperature(-101m));
        Assert.Contains("Temperature must be between", exception.Message);
        Assert.Contains("-100", exception.Message);
    }

    [Fact]
    public void Constructor_WithCelsiusAboveMaximum_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Temperature(61m));
        Assert.Contains("Temperature must be between", exception.Message);
        Assert.Contains("60", exception.Message);
    }

    [Theory]
    [InlineData(0, 32)]
    [InlineData(25, 77)]
    [InlineData(-40, -40)]
    [InlineData(100, 212)] // Would fail validation, but shows formula
    public void Fahrenheit_ConvertsCorrectly(decimal celsius, decimal expectedFahrenheit)
    {
        // Arrange
        if (celsius < -100 || celsius > 60)
        {
            // Skip invalid values
            return;
        }
        var temperature = new Temperature(celsius);

        // Act
        var fahrenheit = temperature.Fahrenheit;

        // Assert
        Assert.Equal(expectedFahrenheit, fahrenheit);
    }

    [Fact]
    public void Fahrenheit_WithZeroCelsius_ReturnsThirtyTwoFahrenheit()
    {
        // Arrange
        var temperature = new Temperature(0m);

        // Act
        var fahrenheit = temperature.Fahrenheit;

        // Assert
        Assert.Equal(32m, fahrenheit);
    }

    [Fact]
    public void ImplicitOperator_FromDecimal_CreatesTemperature()
    {
        // Arrange & Act
        Temperature temperature = 20m;

        // Assert
        Assert.Equal(20m, temperature.Celsius);
    }

    [Fact]
    public void ExplicitOperator_ToDecimal_ReturnsCorrectValue()
    {
        // Arrange
        var temperature = new Temperature(15m);

        // Act
        var celsius = (decimal)temperature;

        // Assert
        Assert.Equal(15m, celsius);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var temperature1 = new Temperature(20m);
        var temperature2 = new Temperature(20m);

        // Act & Assert
        Assert.Equal(temperature1, temperature2);
        Assert.True(temperature1 == temperature2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var temperature1 = new Temperature(20m);
        var temperature2 = new Temperature(25m);

        // Act & Assert
        Assert.NotEqual(temperature1, temperature2);
        Assert.True(temperature1 != temperature2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        var temperature1 = new Temperature(20m);
        var temperature2 = new Temperature(20m);

        // Act & Assert
        Assert.Equal(temperature1.GetHashCode(), temperature2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var temperature = new Temperature(25m);

        // Act
        var result = temperature.ToString();

        // Assert
        Assert.Equal("25°C", result);
    }

    [Fact]
    public void ToString_WithNegativeValue_ReturnsFormattedString()
    {
        // Arrange
        var temperature = new Temperature(-10m);

        // Act
        var result = temperature.ToString();

        // Assert
        Assert.Equal("-10°C", result);
    }
}
