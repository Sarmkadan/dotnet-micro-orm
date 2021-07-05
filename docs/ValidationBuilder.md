# ValidationBuilder
The `ValidationBuilder` type is designed to provide a fluent interface for building and executing validation rules against data. It allows developers to chain multiple validation conditions together, making it easier to validate complex data structures and business rules.

## API
The `ValidationBuilder` type exposes several public members:
* `When`: Specifies a conditional validation rule.
* `NotNull`: Specifies that the value cannot be null.
* `NotEmpty`: Specifies that the value cannot be empty.
* `MinLength`: Specifies the minimum length of the value.
* `MaxLength`: Specifies the maximum length of the value.
* `Range`: Specifies a range of valid values.
* `Email`: Specifies that the value must be a valid email address.
* `Url`: Specifies that the value must be a valid URL.
* `PhoneNumber`: Specifies that the value must be a valid phone number.
* `Regex`: Specifies a custom regular expression validation rule.
* `Custom`: Specifies a custom validation rule.
* `GetErrors`: Returns a list of error messages for the validation rules.
* `ThrowIfInvalid`: Throws an exception if the validation rules are not met.
* `IsValidEmail`: A static method to check if a string is a valid email address.
* `IsValidPhoneNumber`: A static method to check if a string is a valid phone number.
* `IsValidUrl`: A static method to check if a string is a valid URL.
* `IsStrongPassword`: A static method to check if a string is a strong password.
* `IsValidCreditCard`: A static method to check if a string is a valid credit card number.
* `IsValidIPAddress`: A static method to check if a string is a valid IP address.

## Usage
Here are two examples of using the `ValidationBuilder` type:
```csharp
// Example 1: Validating a user's email address
var validation = new ValidationBuilder();
validation.Email("user@example.com");
if (validation.GetErrors().Count > 0)
{
    Console.WriteLine("Invalid email address");
}
else
{
    Console.WriteLine("Valid email address");
}

// Example 2: Validating a user's password
var password = "P@ssw0rd";
var validation = new ValidationBuilder();
validation.MinLength(8).MaxLength(128).Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
if (validation.GetErrors().Count > 0)
{
    Console.WriteLine("Invalid password");
}
else
{
    Console.WriteLine("Valid password");
}
```

## Notes
When using the `ValidationBuilder` type, note that the `ThrowIfInvalid` method will throw an exception if the validation rules are not met. Also, the `GetErrors` method will return a list of error messages for the validation rules. The static methods (`IsValidEmail`, `IsValidPhoneNumber`, etc.) can be used to perform individual validation checks. The `ValidationBuilder` type is designed to be thread-safe, but it is still important to ensure that the validation rules are properly synchronized if accessing shared data. Additionally, the `Custom` method allows for custom validation rules to be implemented, but these rules must be properly tested to ensure correctness.
