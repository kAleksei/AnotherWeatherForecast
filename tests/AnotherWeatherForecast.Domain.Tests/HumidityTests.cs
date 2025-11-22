using AnotherWeatherForecast.Domain.ValueObjects;

namespace AnotherWeatherForecast.Domain.Tests;

public class HumidityTests
{
    [Fact]
    public void Constructor_WithValidPercent_CreatesHumidity()
    {
        // Arrange & Act
        var humidity = new Humidity(50m);

        // Assert
        Assert.Equal(50m, humidity.Percent);
    }

    [Fact]
    public void Constructor_WithMinimumValidPercent_CreatesHumidity()
    {
        // Arrange & Act
        var humidity = new Humidity(0m);

        // Assert
        Assert.Equal(0m, humidity.Percent);
    }

    [Fact]
    public void Constructor_WithMaximumValidPercent_CreatesHumidity()
    {
        // Arrange & Act
        var humidity = new Humidity(100m);

        // Assert
        Assert.Equal(100m, humidity.Percent);
    }

    [Fact]
    public void Constructor_WithPercentBelowMinimum_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Humidity(-1m));
        Assert.Contains("Humidity must be between", exception.Message);
        Assert.Contains("0%", exception.Message);
    }

    [Fact]
    public void Constructor_WithPercentAboveMaximum_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Humidity(101m));
        Assert.Contains("Humidity must be between", exception.Message);
        Assert.Contains("100%", exception.Message);
    }

    [Fact]
    public void ImplicitOperator_FromDecimal_CreatesHumidity()
    {
        // Arrange & Act
        Humidity humidity = 75m;

        // Assert
        Assert.Equal(75m, humidity.Percent);
    }

    [Fact]
    public void ExplicitOperator_ToDecimal_ReturnsCorrectValue()
    {
        // Arrange
        var humidity = new Humidity(65m);

        // Act
        var percent = (decimal)humidity;

        // Assert
        Assert.Equal(65m, percent);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var humidity1 = new Humidity(50m);
        var humidity2 = new Humidity(50m);

        // Act & Assert
        Assert.Equal(humidity1, humidity2);
        Assert.True(humidity1 == humidity2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var humidity1 = new Humidity(50m);
        var humidity2 = new Humidity(75m);

        // Act & Assert
        Assert.NotEqual(humidity1, humidity2);
        Assert.True(humidity1 != humidity2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        var humidity1 = new Humidity(50m);
        var humidity2 = new Humidity(50m);

        // Act & Assert
        Assert.Equal(humidity1.GetHashCode(), humidity2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var humidity = new Humidity(65m);

        // Act
        var result = humidity.ToString();

        // Assert
        Assert.Equal("65%", result);
    }

    [Fact]
    public void ToString_WithZeroPercent_ReturnsFormattedString()
    {
        // Arrange
        var humidity = new Humidity(0m);

        // Act
        var result = humidity.ToString();

        // Assert
        Assert.Equal("0%", result);
    }

    [Fact]
    public void ToString_WithHundredPercent_ReturnsFormattedString()
    {
        // Arrange
        var humidity = new Humidity(100m);

        // Act
        var result = humidity.ToString();

        // Assert
        Assert.Equal("100%", result);
    }
}
