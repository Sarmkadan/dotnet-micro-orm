#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Utils;

using System.Text.RegularExpressions;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Fluent validation builder for entities
/// </summary>
public class sealed ValidationBuilder
{
    private readonly List<string> _errors = [];

    public ValidationBuilder When(bool condition, string errorMessage)
    {
        if (condition)
            _errors.Add(errorMessage);
        return this;
    }

    public ValidationBuilder NotNull(object? value, string propertyName) =>
        When(value is null, $"{propertyName} is required");

    public ValidationBuilder NotEmpty(string? value, string propertyName) =>
        When(string.IsNullOrWhiteSpace(value), $"{propertyName} cannot be empty");

    public ValidationBuilder MinLength(string? value, int minLength, string propertyName) =>
        When(string.IsNullOrEmpty(value) || value.Length < minLength,
            $"{propertyName} must be at least {minLength} characters");

    public ValidationBuilder MaxLength(string? value, int maxLength, string propertyName) =>
        When(!string.IsNullOrEmpty(value) && value.Length > maxLength,
            $"{propertyName} cannot exceed {maxLength} characters");

    public ValidationBuilder Range(int value, int min, int max, string propertyName) =>
        When(value < min || value > max,
            $"{propertyName} must be between {min} and {max}");

    public ValidationBuilder Range(decimal value, decimal min, decimal max, string propertyName) =>
        When(value < min || value > max,
            $"{propertyName} must be between {min} and {max}");

    public ValidationBuilder Email(string? value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
            return this;

        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return When(!Regex.IsMatch(value, emailPattern),
            $"{propertyName} is not a valid email address");
    }

    public ValidationBuilder Url(string? value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
            return this;

        var urlPattern = @"^https?://";
        return When(!Regex.IsMatch(value, urlPattern),
            $"{propertyName} is not a valid URL");
    }

    public ValidationBuilder PhoneNumber(string? value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
            return this;

        var phonePattern = @"^\d{10,}$";
        return When(!Regex.IsMatch(value.Replace("-", "").Replace(" ", ""), phonePattern),
            $"{propertyName} is not a valid phone number");
    }

    public ValidationBuilder Regex(string? value, string pattern, string propertyName, string? message = null)
    {
        if (string.IsNullOrEmpty(value))
            return this;

        var errorMsg = message ?? $"{propertyName} format is invalid";
        return When(!System.Text.RegularExpressions.Regex.IsMatch(value, pattern), errorMsg);
    }

    public ValidationBuilder Custom(bool isValid, string errorMessage) =>
        When(!isValid, errorMessage);

    public List<string> GetErrors() => _errors;

    public bool IsValid => _errors.Count == 0;

    public void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new Exceptions.EntityValidationException("Validation failed", _errors);
    }
}

/// <summary>
/// Common validation rules
/// </summary>
public static class ValidationRules
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 10 && cleaned.Length <= 15;
    }

    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsStrongPassword(string password)
    {
        if (password.Length < 8)
            return false;

        var hasUpperCase = password.Any(char.IsUpper);
        var hasLowerCase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    public static bool IsValidCreditCard(string cardNumber)
    {
        var cleaned = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (cleaned.Length < 13 || cleaned.Length > 19)
            return false;

        return LuhnCheck(cleaned);
    }

    private static bool LuhnCheck(string cardNumber)
    {
        var sum = 0;
        var isEven = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            var digit = cardNumber[i] - '0';

            if (isEven)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 == 0;
    }

    public static bool IsValidIPAddress(string ipAddress)
    {
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }

    public static bool IsValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }
}

/// <summary>
/// Entity validation extension
/// </summary>
public static class ValidationExtensions
{
    public static ValidationBuilder CreateValidator(this BaseEntity entity)
    {
        return new ValidationBuilder();
    }

    public static ValidationBuilder NotModified(this ValidationBuilder validator, object original, object current, string propertyName)
    {
        if (!Equals(original, current))
            validator.When(true, $"{propertyName} cannot be modified");
        return validator;
    }

    public static ValidationBuilder Custom<T>(this ValidationBuilder validator, Func<T, bool> rule, T value, string message)
    {
        return validator.When(!rule(value), message);
    }
}
