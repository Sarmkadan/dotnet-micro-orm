# ValidationHelperTests

The `ValidationHelperTests` class contains unit tests for the `ValidationHelper` component in the `dotnet-micro-orm` project. Each test method verifies a specific validation rule for passwords or email addresses, asserting that the helper returns the expected success or error result under given inputs. The class is intended to be run by a test framework (e.g., xUnit, NUnit) and does not expose any public fields, properties, or constructors beyond the test methods listed.

## API

### `ValidatePassword_StrongPassword_ReturnsSuccess`

- **Purpose**: Verifies that a strong password (meeting all defined strength criteria) passes validation.
- **Parameters**: None. The test supplies a hard‑coded strong password internally.
- **Return value**: `void`. The test passes if the validation result indicates success; otherwise it fails with an assertion exception.
- **Throws**: Only assertion exceptions from the testing framework if the validation does not return success.

### `ValidatePassword_NullInput_ReturnsRequiredError`

- **Purpose**: Verifies that a `null` password input produces a “required” validation error.
- **Parameters**: None. The test passes `null` to the validation method.
- **Return value**: `void`. The test passes if the validation result contains a required‑field error; otherwise it fails.
- **Throws**: Only assertion exceptions if the expected error is not present.

### `ValidateEmail_ValidEmail_ReturnsSuccess`

- **Purpose**: Verifies that a syntactically valid email address passes validation.
- **Parameters**: None. The test uses a hard‑coded valid email string.
- **Return value**: `void`. The test passes if the validation result indicates success.
- **Throws**: Only assertion exceptions on failure.

### `ValidateEmail_NoAtSymbol_ReturnsInvalidFormatError`

- **Purpose**: Verifies that an email address missing the `@` symbol triggers an “invalid format” error.
- **Parameters**: None. The test supplies an email string without `@`.
- **Return value**: `void`. The test passes if the validation result includes an invalid‑format error.
- **Throws**: Only assertion exceptions if the error is missing or incorrect.

## Usage

The following examples demonstrate how to run these tests using a typical test runner. The tests are self‑contained and require no external setup.

```csharp
// Example 1: Running all password validation tests
[Fact]
public void RunPasswordTests()
{
    var tests = new ValidationHelperTests();
    
    // These methods will throw on failure
    tests.ValidatePassword_StrongPassword_ReturnsSuccess();
    tests.ValidatePassword_NullInput_ReturnsRequiredError();
}
```

```csharp
// Example 2: Running email validation tests individually
[Fact]
public void RunEmailTests()
{
    var tests = new ValidationHelperTests();
    
    tests.ValidateEmail_ValidEmail_ReturnsSuccess();
    tests.ValidateEmail_NoAtSymbol_ReturnsInvalidFormatError();
}
```

In a real test suite, each method would be decorated with a test attribute (e.g., `[Fact]` or `[Test]`) and executed by the framework directly. The examples above show manual invocation for illustration.

## Notes

- **Edge cases**: The tests cover only the specific scenarios described. For password validation, edge cases such as empty strings, whitespace‑only input, or passwords that almost meet strength criteria are not tested by these methods. Similarly, email validation does not cover addresses with multiple `@` symbols, missing domain, or international characters. Additional tests would be needed for comprehensive coverage.
- **Thread safety**: `ValidationHelperTests` is a test class and is not designed for concurrent execution. Its methods are intended to run sequentially within a single test session. No shared mutable state exists between tests, so thread‑safety concerns are irrelevant under normal single‑threaded test runners. If tests were executed in parallel, each test would operate on its own instance of the class, avoiding race conditions.
- **Dependencies**: The tests assume the existence of a `ValidationHelper` class (or equivalent) with methods that accept a password or email string and return a validation result object. The exact API of that helper is not exposed here; these tests validate its behavior indirectly.
