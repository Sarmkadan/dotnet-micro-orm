## ValidationHelper

The `ValidationHelper` class provides utility methods for validating common data types and business rules. It helps ensure data integrity before database operations and API calls by returning detailed error messages for validation failures. Each validation method returns a tuple with a boolean indicating success/failure and an error message that can be used for user feedback.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotnetMicroOrm.Utils;

class Program
{
    static void Main()
    {
        // Validate required fields
        var (isValid, error) = ValidationHelper.ValidateRequired(null, "Username");
        Console.WriteLine($"Required validation: isValid={isValid}, error='{error}'");
        
        // Validate string length
        var lengthResult = ValidationHelper.ValidateLength("Hello", 3, 10, "Username");
        Console.WriteLine($"Length validation: isValid={lengthResult.isValid}, error='{lengthResult.errorMessage}'");
        
        // Validate email format
        var emailResult = ValidationHelper.ValidateEmail("user@example.com");
        Console.WriteLine($"Email validation: isValid={emailResult.isValid}, error='{emailResult.errorMessage}'");
        
        // Validate password strength
        var passwordResult = ValidationHelper.ValidatePassword("StrongPass123!");
        Console.WriteLine($"Password validation: isValid={passwordResult.isValid}, error='{passwordResult.errorMessage}'");
        
        // Validate numeric range
        var rangeResult = ValidationHelper.ValidateRange(42, 1, 100, "Age");
        Console.WriteLine($"Range validation: isValid={rangeResult.isValid}, error='{rangeResult.errorMessage}'");
        
        // Validate decimal range
        var decimalRangeResult = ValidationHelper.ValidateRange(99.99m, 0, 1000, "Price");
        Console.WriteLine($"Decimal range validation: isValid={decimalRangeResult.isValid}, error='{decimalRangeResult.errorMessage}'");
        
        // Validate positive value
        var positiveResult = ValidationHelper.ValidatePositive(10.5m, "Quantity");
        Console.WriteLine($"Positive validation: isValid={positiveResult.isValid}, error='{positiveResult.errorMessage}'");
        
        // Validate non-negative value
        var nonNegativeResult = ValidationHelper.ValidateNonNegative(0, "Discount");
        Console.WriteLine($"Non-negative validation: isValid={nonNegativeResult.isValid}, error='{nonNegativeResult.errorMessage}'");
        
        // Validate phone number
        var phoneResult = ValidationHelper.ValidatePhoneNumber("+1234567890");
        Console.WriteLine($"Phone validation: isValid={phoneResult.isValid}, error='{phoneResult.errorMessage}'");
        
        // Validate URL
        var urlResult = ValidationHelper.ValidateUrl("https://example.com");
        Console.WriteLine($"URL validation: isValid={urlResult.isValid}, error='{urlResult.errorMessage}'");
        
        // Validate non-empty collection
        var collectionResult = ValidationHelper.ValidateNotEmpty(new List<string> { "item1", "item2" }, "Items");
        Console.WriteLine($"Collection validation: isValid={collectionResult.isValid}, error='{collectionResult.errorMessage}'");
        
        // Validate collection size
        var collectionSizeResult = ValidationHelper.ValidateCollectionSize(new[] { 1, 2, 3 }, 2, 5, "Tags");
        Console.WriteLine($"Collection size validation: isValid={collectionSizeResult.isValid}, error='{collectionSizeResult.errorMessage}'");
        
        // Validate future date
        var futureDateResult = ValidationHelper.ValidateFutureDate(DateTime.UtcNow.AddDays(1), "EventDate");
        Console.WriteLine($"Future date validation: isValid={futureDateResult.isValid}, error='{futureDateResult.errorMessage}'");
        
        // Validate past date
        var pastDateResult = ValidationHelper.ValidatePastDate(DateTime.UtcNow.AddDays(-1), "BirthDate");
        Console.WriteLine($"Past date validation: isValid={pastDateResult.isValid}, error='{pastDateResult.errorMessage}'");
        
        // Validate multiple rules with ValidateAll
        var validations = new (bool isValid, string errorMessage)[
        ]
        {
            ValidationHelper.ValidateRequired("username", "Username"),
            ValidationHelper.ValidateLength("username", 3, 50, "Username"),
            ValidationHelper.ValidateEmail("user@example.com")
        };
        
        var allValid = ValidationHelper.ValidateAll(validations);
        Console.WriteLine($"All validations: isValid={allValid.isValid}, error='{allValid.errorMessage}'");
    }
}
```

## StringHelper

`StringHelper` offers a collection of static methods for common string transformations and checks, such as case conversion, truncation, repetition, whitespace handling, character classification, and simple validation of emails and URLs.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotnetMicroOrm.Utils;

class Program
{
    static void Main()
    {
        // Case conversions
        string kebab = StringHelper.ToKebabCase("UserProfile");          // "user-profile"
        string snake = StringHelper.ToSnakeCase("UserProfile");          // "user_profile"
        string pascal = StringHelper.ToPascalCase("user-profile");       // "UserProfile"

        // Truncate and repeat
        string truncated = StringHelper.Truncate("Hello World", 5);      // "He..."
        string repeated = "ab".Repeat(3);                               // "ababab"

        // Whitespace removal
        string noWs = " a b c ".RemoveWhitespace();                     // "abc"

        // Character checks
        bool hasUpper = "Hello".ContainsUpperCase();                    // true
        bool hasLower = "HELLO".ContainsLowerCase();                    // false
        bool hasDigit = "Pass123".ContainsDigit();                      // true
        bool alnum = "ABC123".IsAlphanumeric();                         // true

        // Validation helpers
        bool emailOk = "user@example.com".IsValidEmail();               // true
        bool urlOk = "https://example.com".IsValidUrl();                // true

        // Comparison and pluralization
        bool equalIgnoreCase = "test".EqualsIgnoreCase("TEST");         // true
        string plural = "city".Pluralize(2);                            // "cities"

        // Reversal and substring extraction
        string reversed = "abc".Reverse();                              // "cba"
        string left = "abcdef".Left(3);                                 // "abc"
        string right = "abcdef".Right(2);                               // "ef"

        // Multiple replacements in one pass
        var replacements = new Dictionary<string, string>
        {
            ["Hello"] = "Hi",
            ["World"] = "Universe"
        };
        string replaced = "Hello World".ReplaceMultiple(replacements); // "Hi Universe"

        // Output results
        Console.WriteLine($"kebab: {kebab}");
        Console.WriteLine($"snake: {snake}");
        Console.WriteLine($"pascal: {pascal}");
        Console.WriteLine($"truncated: {truncated}");
        Console.WriteLine($"repeated: {repeated}");
        Console.WriteLine($"noWs: {noWs}");
        Console.WriteLine($"hasUpper: {hasUpper}, hasLower: {hasLower}, hasDigit: {hasDigit}, alnum: {alnum}");
        Console.WriteLine($"emailOk: {emailOk}, urlOk: {urlOk}");
        Console.WriteLine($"equalIgnoreCase: {equalIgnoreCase}, plural: {plural}");
        Console.WriteLine($"reversed: {reversed}, left: {left}, right: {right}");
        Console.WriteLine($"replaced: {replaced}");
    }
}
```
