# ValidationHelper

The `ValidationHelper` class provides a set of static methods for common input validation scenarios. Each method returns a tuple `(bool isValid, string errorMessage)` indicating whether the validation passed and, if not, a human-readable error message. The class is designed for use in data access layers, API endpoints, and business logic where lightweight, reusable validation is needed without throwing exceptions for expected invalid input.

## API

### `ValidateRequired`
Validates that a value is not null, empty, or whitespace (for strings).  
**Parameters:**  
- `value` (string?) – The value to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the value is non-null and contains non-whitespace characters; otherwise `false` with an appropriate error message.  
**Throws:** None.

### `ValidateLength`
Validates that a string’s length falls within a specified inclusive range.  
**Parameters:**  
- `value` (string?) – The string to validate.  
- `minLength` (int) – Minimum allowed length.  
- `maxLength` (int) – Maximum allowed length.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the string is not null and its length is between `minLength` and `maxLength` (inclusive); otherwise `false`.  
**Throws:** `ArgumentOutOfRangeException` if `minLength` > `maxLength`.

### `ValidateEmail`
Validates that a string is a syntactically valid email address using a regular expression.  
**Parameters:**  
- `value` (string?) – The email address to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the value matches a standard email pattern; otherwise `false`.  
**Throws:** None.

### `ValidatePassword`
Validates that a password meets common complexity requirements: minimum length, at least one uppercase letter, one lowercase letter, one digit, and one special character.  
**Parameters:**  
- `value` (string?) – The password to validate.  
- `minLength` (int) – Minimum required length (default 8).  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if all criteria are met; otherwise `false` with a message describing the missing requirement.  
**Throws:** `ArgumentOutOfRangeException` if `minLength` < 1.

### `ValidateRange` (numeric overload)
Validates that a numeric value is within an inclusive range.  
**Parameters:**  
- `value` (IComparable) – The numeric value to validate.  
- `min` (IComparable) – Lower bound.  
- `max` (IComparable) – Upper bound.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if `min <= value <= max`; otherwise `false`.  
**Throws:** `ArgumentException` if `min` > `max`.

### `ValidateRange` (date overload)
Validates that a `DateTime` value falls within an inclusive date range.  
**Parameters:**  
- `value` (DateTime?) – The date to validate.  
- `min` (DateTime) – Earliest allowed date.  
- `max` (DateTime) – Latest allowed date.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the value is not null and `min <= value <= max`; otherwise `false`.  
**Throws:** `ArgumentException` if `min` > `max`.

### `ValidatePositive`
Validates that a numeric value is greater than zero.  
**Parameters:**  
- `value` (IComparable) – The value to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if `value > 0`; otherwise `false`.  
**Throws:** None.

### `ValidateNonNegative`
Validates that a numeric value is zero or greater.  
**Parameters:**  
- `value` (IComparable) – The value to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if `value >= 0`; otherwise `false`.  
**Throws:** None.

### `ValidatePhoneNumber`
Validates that a string is a valid phone number in a common international format (e.g., E.164 or with country code).  
**Parameters:**  
- `value` (string?) – The phone number to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the value matches a phone number pattern; otherwise `false`.  
**Throws:** None.

### `ValidateUrl`
Validates that a string is a well-formed absolute URL (http/https).  
**Parameters:**  
- `value` (string?) – The URL to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the value can be parsed as an absolute URI with a scheme of http or https; otherwise `false`.  
**Throws:** None.

### `ValidateNotEmpty<T>`
Validates that a collection is not null and contains at least one element.  
**Type parameters:**  
- `T` – The element type of the collection.  
**Parameters:**  
- `collection` (IEnumerable<T>?) – The collection to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the collection is not null and has at least one element; otherwise `false`.  
**Throws:** None.

### `ValidateCollectionSize<T>`
Validates that a collection’s element count falls within a specified inclusive range.  
**Type parameters:**  
- `T` – The element type of the collection.  
**Parameters:**  
- `collection` (IEnumerable<T>?) – The collection to validate.  
- `minSize` (int) – Minimum allowed count.  
- `maxSize` (int) – Maximum allowed count.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the collection is not null and its count is between `minSize` and `maxSize` (inclusive); otherwise `false`.  
**Throws:** `ArgumentOutOfRangeException` if `minSize` > `maxSize`.

### `ValidateFutureDate`
Validates that a `DateTime` value is in the future (strictly greater than the current UTC time).  
**Parameters:**  
- `value` (DateTime?) – The date to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the value is not null and `value > DateTime.UtcNow`; otherwise `false`.  
**Throws:** None.

### `ValidatePastDate`
Validates that a `DateTime` value is in the past (strictly less than the current UTC time).  
**Parameters:**  
- `value` (DateTime?) – The date to validate.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` if the value is not null and `value < DateTime.UtcNow`; otherwise `false`.  
**Throws:** None.

### `ValidateAll`
Executes multiple validation methods and aggregates results.  
**Parameters:**  
- `validators` (params Func<(bool, string)>[]) – One or more validation delegates to execute.  
**Returns:** `(bool isValid, string errorMessage)` – `isValid` is `true` only if all validators return `true`; otherwise `false` with the first error message encountered.  
**Throws:** `ArgumentNullException` if any delegate is null.

## Usage

### Example 1: Validating a user registration input

```csharp
using DotNetMicroOrm.Validation;

public (bool success, string error) RegisterUser(string name, string email, string password, int age)
{
    var validations = new Func<(bool, string)>[]
    {
        () => ValidationHelper.ValidateRequired(name),
        () => ValidationHelper.ValidateLength(name, 2, 50),
        () => ValidationHelper.ValidateEmail(email),
        () => ValidationHelper.ValidatePassword(password, minLength: 8),
        () => ValidationHelper.ValidateRange(age, 18, 120)
    };

    return ValidationHelper.ValidateAll(validations);
}
```

### Example 2: Validating a product with collections and dates

```csharp
using DotNetMicroOrm.Validation;

public (bool valid, string message) ValidateProduct(
    string title,
    decimal price,
    List<string> tags,
    DateTime? releaseDate)
{
    var result = ValidationHelper.ValidateAll(
        () => ValidationHelper.ValidateRequired(title),
        () => ValidationHelper.ValidatePositive(price),
        () => ValidationHelper.ValidateNotEmpty(tags),
        () => ValidationHelper.ValidateCollectionSize(tags, 1, 10),
        () => ValidationHelper.ValidateFutureDate(releaseDate)
    );

    return result;
}
```

## Notes

- **Null handling:** All methods accept nullable parameters where applicable. A `null` value for a required field will cause `ValidateRequired` to return `false`. For methods like `ValidateLength`, `ValidateEmail`, etc., a `null` input is treated as invalid and returns `false` with an appropriate message.
- **Thread safety:** All methods are static and stateless. They do not modify any shared state and are safe to call concurrently from multiple threads. Regular expression patterns used internally (e.g., for email, phone, URL) are compiled once and cached.
- **Date comparisons:** `ValidateFutureDate` and `ValidatePastDate` use `DateTime.UtcNow` as the reference point. To avoid time zone ambiguity, consider converting local times to UTC before validation.
- **Performance:** `ValidateAll` short-circuits on the first failure, so order validators from cheapest to most expensive when possible.
- **Overloads:** The two `ValidateRange` overloads are distinguished by the types of their parameters (numeric vs. date). The compiler resolves the correct overload based on argument types.
