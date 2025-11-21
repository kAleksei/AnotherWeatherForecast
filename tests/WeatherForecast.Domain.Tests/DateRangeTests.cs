using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Domain.Tests;

public class DateRangeTests
{
    [Fact]
    public void Constructor_WithValidDates_CreatesDateRange()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today;
        var endDate = today.AddDays(3);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        Assert.Equal(startDate, dateRange.StartDate);
        Assert.Equal(endDate, dateRange.EndDate);
    }

    [Fact]
    public void Constructor_WithSameStartAndEndDate_CreatesDateRange()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        // Act
        var dateRange = new DateRange(today, today);

        // Assert
        Assert.Equal(today, dateRange.StartDate);
        Assert.Equal(today, dateRange.EndDate);
        Assert.Equal(1, dateRange.Days);
    }

    [Fact]
    public void Constructor_WithStartDateSevenDaysAgo_CreatesDateRange()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-7);
        var endDate = today;

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        Assert.Equal(startDate, dateRange.StartDate);
        Assert.Equal(endDate, dateRange.EndDate);
    }

    [Fact]
    public void Constructor_WithEndDateSevenDaysInFuture_CreatesDateRange()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today;
        var endDate = today.AddDays(7);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        Assert.Equal(startDate, dateRange.StartDate);
        Assert.Equal(endDate, dateRange.EndDate);
    }

    [Fact]
    public void Constructor_WithStartDateEightDaysAgo_ThrowsArgumentException()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-8);
        var endDate = today;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DateRange(startDate, endDate));
        Assert.Contains("Start date must be within", exception.Message);
    }

    [Fact]
    public void Constructor_WithEndDateEightDaysInFuture_ThrowsArgumentException()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today;
        var endDate = today.AddDays(8);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DateRange(startDate, endDate));
        Assert.Contains("End date must be within", exception.Message);
    }

    [Fact]
    public void Constructor_WithEndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(2);
        var endDate = today;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DateRange(startDate, endDate));
        Assert.Contains("End date", exception.Message);
        Assert.Contains("cannot be before start date", exception.Message);
    }

    [Fact]
    public void Constructor_NormalizesToDate_IgnoresTime()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddHours(12).AddMinutes(30);
        var endDate = today.AddDays(1).AddHours(15).AddMinutes(45);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        Assert.Equal(today, dateRange.StartDate);
        Assert.Equal(today.AddDays(1), dateRange.EndDate);
        Assert.Equal(TimeSpan.Zero, dateRange.StartDate.TimeOfDay);
        Assert.Equal(TimeSpan.Zero, dateRange.EndDate.TimeOfDay);
    }

    [Fact]
    public void Days_WithSingleDay_ReturnsOne()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var dateRange = new DateRange(today, today);

        // Act
        var days = dateRange.Days;

        // Assert
        Assert.Equal(1, days);
    }

    [Fact]
    public void Days_WithMultipleDays_ReturnsCorrectCount()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var dateRange = new DateRange(today, today.AddDays(4));

        // Act
        var days = dateRange.Days;

        // Assert
        Assert.Equal(5, days);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var dateRange1 = new DateRange(today, today.AddDays(3));
        var dateRange2 = new DateRange(today, today.AddDays(3));

        // Act & Assert
        Assert.Equal(dateRange1, dateRange2);
        Assert.True(dateRange1 == dateRange2);
    }

    [Fact]
    public void Equals_WithDifferentStartDate_ReturnsFalse()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var dateRange1 = new DateRange(today, today.AddDays(3));
        var dateRange2 = new DateRange(today.AddDays(1), today.AddDays(3));

        // Act & Assert
        Assert.NotEqual(dateRange1, dateRange2);
        Assert.True(dateRange1 != dateRange2);
    }

    [Fact]
    public void Equals_WithDifferentEndDate_ReturnsFalse()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var dateRange1 = new DateRange(today, today.AddDays(3));
        var dateRange2 = new DateRange(today, today.AddDays(4));

        // Act & Assert
        Assert.NotEqual(dateRange1, dateRange2);
        Assert.True(dateRange1 != dateRange2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var dateRange1 = new DateRange(today, today.AddDays(2));
        var dateRange2 = new DateRange(today, today.AddDays(2));

        // Act & Assert
        Assert.Equal(dateRange1.GetHashCode(), dateRange2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var startDate = today;
        var endDate = today.AddDays(5);
        var dateRange = new DateRange(startDate, endDate);

        // Act
        var result = dateRange.ToString();

        // Assert
        Assert.Equal($"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", result);
    }
}
