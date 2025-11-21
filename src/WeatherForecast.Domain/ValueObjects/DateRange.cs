namespace WeatherForecast.Domain.ValueObjects;

/// <summary>
/// Represents a date range for weather forecasts.
/// </summary>
public sealed record DateRange : IEquatable<DateRange>
{
    private const int MaxDaysFromToday = 7;

    /// <summary>
    /// Gets the start date of the range.
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// Gets the end date of the range.
    /// </summary>
    public DateTime EndDate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateRange"/> class.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <exception cref="ArgumentException">Thrown when dates are outside the valid range (±7 days from today) or when end date is before start date.</exception>
    public DateRange(DateTime startDate, DateTime endDate)
    {
        var today = DateTime.UtcNow.Date;
        var minAllowedDate = today.AddDays(-MaxDaysFromToday);
        var maxAllowedDate = today.AddDays(MaxDaysFromToday);

        if (startDate.Date < minAllowedDate || startDate.Date > maxAllowedDate)
        {
            throw new ArgumentException(
                $"Start date must be within ±{MaxDaysFromToday} days from today. Provided: {startDate:yyyy-MM-dd}, Allowed range: {minAllowedDate:yyyy-MM-dd} to {maxAllowedDate:yyyy-MM-dd}",
                nameof(startDate));
        }

        if (endDate.Date < minAllowedDate || endDate.Date > maxAllowedDate)
        {
            throw new ArgumentException(
                $"End date must be within ±{MaxDaysFromToday} days from today. Provided: {endDate:yyyy-MM-dd}, Allowed range: {minAllowedDate:yyyy-MM-dd} to {maxAllowedDate:yyyy-MM-dd}",
                nameof(endDate));
        }

        if (endDate.Date < startDate.Date)
        {
            throw new ArgumentException(
                $"End date ({endDate:yyyy-MM-dd}) cannot be before start date ({startDate:yyyy-MM-dd}).",
                nameof(endDate));
        }

        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    /// <summary>
    /// Gets the number of days in the range (inclusive).
    /// </summary>
    public int Days => (EndDate - StartDate).Days + 1;

    /// <summary>
    /// Returns a string representation of the date range.
    /// </summary>
    /// <returns>A string in the format "StartDate to EndDate".</returns>
    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
