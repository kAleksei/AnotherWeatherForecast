using FluentValidation;
using System.Text.RegularExpressions;
using WeatherForecast.Application.Common.Models;

namespace WeatherForecast.Application.Validators;

/// <summary>
/// Validator for weather forecast requests.
/// </summary>
public class WeatherForecastRequestValidator : AbstractValidator<WeatherForecastRequest>
{
    private static readonly Regex CountryCodeRegex = new(@"^[A-Z]{2}$", RegexOptions.Compiled);
    private const int MaxFutureDays = 7;
    private const  int MaxPastDays = 60;

    public WeatherForecastRequestValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City name cannot exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .Must(BeValidCountryCode).WithMessage("Country must be a 2-letter uppercase ISO 3166-1 alpha-2 code (e.g., 'US', 'GB').");

        RuleFor(x => x.Date)
            .Must(BeWithinAllowedRange).WithMessage($"Date must not be more than {MaxFutureDays} days in the future. Past dates limit is {MaxPastDays} days.");
    }

    private bool BeValidCountryCode(string country)
    {
        return !string.IsNullOrWhiteSpace(country) && CountryCodeRegex.IsMatch(country);
    }

    private bool BeWithinAllowedRange(DateTime date)
    {
        var today = DateTime.UtcNow.Date;
        var maxFutureDate = today.AddDays(MaxFutureDays);
        var maxPastDate = today.AddDays(MaxPastDays * -1); 
        
        return date.Date <= maxFutureDate && date.Date >= maxPastDate;
    }
}
