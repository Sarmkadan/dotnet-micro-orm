# StringHelper

A static utility class providing common string manipulation and validation methods for use in data transformation, input sanitization, and formatting operations within the dotnet-micro-orm library.

## API

### `ToKebabCase(string input)`
Converts the input string to kebab-case format (lowercase words separated by hyphens).  
**Parameters**:  
- `input` (string): The string to convert.  
**Returns**:  
- `string`: The kebab-cased string.  
**Exceptions**:  
- None; returns `null` if `input` is `null`.

---

### `ToSnakeCase(string input)`
Converts the input string to snake_case format (lowercase words separated by underscores).  
**Parameters**:  
- `input` (string): The string to convert.  
**Returns**:  
- `string`: The snake_cased string.  
**Exceptions**:  
- None; returns `null` if `input` is `null`.

---

### `ToPascalCase(string input)`
Converts the input string to PascalCase format (each word capitalized, no separators).  
**Parameters**:  
- `input` (string): The string to convert.  
**Returns**:  
- `string`: The PascalCased string.  
**Exceptions**:  
- None; returns `null` if `input` is `null`.

---

### `Truncate(string input, int maxLength)`
Truncates the input string to the specified maximum length.  
**Parameters**:  
- `input` (string): The string to truncate.  
- `maxLength` (int): The maximum allowed length.  
**Returns**:  
- `string`: The truncated string.  
**Exceptions**:  
- `ArgumentOutOfRangeException`: Thrown when `maxLength` is negative.

---

### `Repeat(string input, int count)`
Repeats the input string the specified number of times.  
**Parameters**:  
- `input` (string): The string to repeat.  
- `count` (int): The number of repetitions.  
**Returns**:  
- `string`: The repeated string.  
**Exceptions**:  
- `ArgumentOutOfRangeException`: Thrown when `count` is negative.

---

### `RemoveWhitespace(string input)`
Removes all whitespace characters from the input string.  
**Parameters**:  
- `input` (string): The string to process.  
**Returns**:  
- `string`: The string with whitespace removed.  
**Exceptions**:  
- None; returns `null` if `input` is `null`.

---

### `ContainsUpperCase(string input)`
Determines whether the input string contains any uppercase letters.  
**Parameters**:  
- `input` (string): The string to check.  
**Returns**:  
- `bool`: `true` if uppercase letters are present; otherwise, `false`.  
**Exceptions**:  
- None; returns `false` if `input` is `null` or empty.

---

### `ContainsLowerCase(string input)`
Determines whether the input string contains any lowercase letters.  
**Parameters**:  
- `input` (string): The string to check.  
**Returns**:  
- `bool`: `true` if lowercase letters are present; otherwise, `false`.  
**Exceptions**:  
- None; returns `false` if `input` is `null` or empty.

---

### `ContainsDigit(string input)`
Determines whether the input string contains any numeric digits.  
**Parameters**:  
- `input` (string): The string to check.  
**Returns**:  
- `bool`: `true` if digits are present; otherwise, `false`.  
**Exceptions**:  
- None; returns `false` if `input` is `null` or empty.

---

### `IsAlphanumeric(string input)`
Determines whether the input string consists exclusively of alphanumeric characters.  
**Parameters**:  
- `input` (string): The string to check.  
**Returns**:  
- `bool`: `true` if all characters are alphanumeric; otherwise, `false`.  
**Exceptions**:  
- None; returns `false` if `input` is `null` or empty.

---

### `IsValidEmail(string input)`
Validates whether the input string conforms to a standard email format.  
**Parameters**:  
- `input` (string): The string to validate.  
**Returns**:  
- `bool`: `true` if the string is a valid email; otherwise, `false`.  
**Exceptions**:  
- None; returns `false` if `input` is `null`.

---

### `IsValidUrl(string input)`
Validates whether the input string conforms to a standard URL format.  
**Parameters**:  
- `input` (string): The string to validate.  
**Returns**:  
- `bool`: `true` if the string is a valid URL; otherwise, `false`.  
**Exceptions**:  
- None; returns `false` if `input` is `null`.

---

### `EqualsIgnoreCase(string a, string b)`
Compares two strings for equality while ignoring case differences.  
**Parameters**:  
- `a` (string): The first string.  
- `b` (string): The second string.  
**Returns**:  
- `bool`: `true` if the strings are equal ignoring case; otherwise, `false`.  
**Exceptions**:  
- None; returns `true` if both strings are `null`.

---

### `Pluralize(string singular)`
Converts a singular noun to its plural form using basic English rules.  
**Parameters**:  
- `singular` (string): The singular noun to pluralize.  
**Returns**:  
- `string`: The pluralized string.  
**Exceptions**:  
- None; returns `null` if `singular` is `null`.

---

### `Reverse(string input)`
Reverses the order of characters in the input string.  
**Parameters**:  
- `input` (string): The string to reverse.  
**Returns**:  
- `string`: The reversed string.  
**Exceptions**:  
- None; returns `null` if `input` is `null`.

---

### `Left(string input, int length)`
Extracts the leftmost `length` characters from the input string.  
**Parameters**:  
- `input` (string): The string to process.  
- `length` (int): The number of characters to extract.  
**Returns**:  
- `string`: The leftmost substring.  
**Exceptions**:  
- `ArgumentOutOfRangeException`: Thrown when `length` is negative.

---

### `Right(string input, int length)`
Extracts the rightmost `length` characters from the input string.  
**Parameters**:  
- `input` (string): The string to process.  
- `length` (int): The number of characters to extract.  
**Returns**:  
- `string`: The rightmost substring.  
**Exceptions**:  
- `ArgumentOutOfRangeException`: Thrown when `length` is negative.

---

### `ReplaceMultiple(string input, string[] oldValues, string[] newValues)`
Replaces multiple substrings in the input string with corresponding replacements.  
**Parameters**:  
- `input` (string): The original string.  
- `oldValues` (string[]): Array of substrings to replace.  
- `newValues` (string[]): Array of replacement substrings.  
**Returns**:  
- `string`: The modified string.  
**Exceptions**:  
- `ArgumentException`: Thrown when `oldValues` and `newValues` arrays have different lengths.

---

## Usage

```csharp
// Convert and validate user input
string userName = "JohnDoe123";
string formattedName = StringHelper.ToKebabCase(userName); // "john-doe-123"
bool hasUppercase = StringHelper.ContainsUpperCase(userName); // true
bool isValidEmail = StringHelper.IsValidEmail("user@example.com"); // true

// Truncate and sanitize text
string description = "This is a very long product description...";
string truncated = StringHelper.Truncate(description, 20); // "This is a very long..."
string sanitized = StringHelper.RemoveWhitespace(truncated); // "Thisisaverylong..."
```

```csharp
// Generate pluralized labels and extract substrings
string singularLabel = "Product";
string pluralLabel = StringHelper.Pluralize(singularLabel); // "Products"
string fileId = "FILE12345";
string numericPart = StringHelper.Right(fileId, 5); // "345"
```

---

## Notes

- All methods are thread-safe due to their stateless, static nature.  
- Null inputs typically result in null outputs or `false` returns unless explicitly handled (e.g., `EqualsIgnoreCase`).  
- `Pluralize` uses simplified rules and may not handle irregular plurals (e.g., "child" → "childs").  
- `ReplaceMultiple` requires matching array lengths for `oldValues` and `newValues`; mismatched lengths throw `ArgumentException`.  
- `Truncate`, `Left`, and `Right` throw `ArgumentOutOfRangeException` for negative length parameters.
