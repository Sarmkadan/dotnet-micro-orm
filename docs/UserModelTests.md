# UserModelTests

Unit test class for the `UserModel` type, covering validation logic, name formatting, and state‑changing operations.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `Validate_WithValidUser_ReturnsTrue` | Confirms that a fully populated `UserModel` passes validation. | none | `void` (test succeeds if validation returns `true`) | Throws an assertion exception if validation fails. |
| `Validate_WithEmptyUsername_ReturnsFalseWithError` | Verifies that an empty username triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected username error. |
| `Validate_WithNullUsername_ReturnsFalseWithError` | Verifies that a `null` username triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected username error. |
| `Validate_WithShortUsername_ReturnsFalseWithError` | Verifies that a username shorter than the allowed length triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected username error. |
| `Validate_WithInvalidEmail_ReturnsFalseWithError` | Verifies that an improperly formatted email triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected email error. |
| `Validate_WithEmptyEmail_ReturnsFalseWithError` | Verifies that an empty email triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected email error. |
| `Validate_WithNullEmail_ReturnsFalseWithError` | Verifies that a `null` email triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected email error. |
| `Validate_WithShortPasswordHash_ReturnsFalseWithError` | Verifies that a password hash shorter than the required length triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected password hash error. |
| `Validate_WithEmptyPasswordHash_ReturnsFalseWithError` | Verifies that an empty password hash triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected password hash error. |
| `Validate_WithLongFirstName_ReturnsFalseWithError` | Verifies that a first name exceeding the maximum length triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected first name error. |
| `Validate_WithLongLastName_ReturnsFalseWithError` | Verifies that a last name exceeding the maximum length triggers validation failure with an appropriate error message. | none | `void` | Throws an assertion exception if validation does not return `false` or if the error collection does not contain the expected last name error. |
| `Validate_WithValidFirstAndLastNames_IncludesInValidation` | Ensures that valid first and last names are considered during validation and do not cause errors. | none | `void` | Throws an assertion exception if validation incorrectly flags the names as invalid. |
| `Validate_WithMultipleErrors_ReturnsAllErrors` | Checks that when several fields are invalid, all corresponding errors are collected and returned. | none | `void` | Throws an assertion exception if the error collection does not contain all expected errors. |
| `GetFullName_WithFirstAndLastNames_ReturnsCombined` | Confirms that `GetFullName` returns the correctly formatted "First Last" string when both names are present. | none | `void` | Throws an assertion exception if the returned string does not match the expected combination. |
| `GetFullName_WithOnlyFirstName_ReturnsFirstName` | Confirms that `GetFullName` returns only the first name when the last name is empty or null. | none | `void` | Throws an assertion exception if the returned string does not equal the first name. |
| `GetFullName_WithOnlyLastName_ReturnsLastName` | Confirms that `GetFullName` returns only the last name when the first name is empty or null. | none | `void` | Throws an assertion exception if the returned string does not equal the last name. |
| `GetFullName_WithNoNames_ReturnsEmpty` | Confirms that `GetFullName` returns an empty string when both first and last names are empty or null. | none | `void` | Throws an assertion exception if the returned string is not empty. |
| `MarkAsEmailVerified_ChangesEmailVerificationFlag` | Verifies that calling `MarkAsEmailVerified` sets the internal email‑verified flag to `true`. | none | `void` | Throws an assertion exception if the flag remains `false` after the call. |
| `UpdateLastLogin_SetsLastLoginDate` | Verifies that `UpdateLastLogin` assigns the supplied date to the `LastLogin` property. | none | `void` | Throws an assertion exception if the `LastLogin` property does not match the provided date. |
| `UpdateLastLogin_MultipleUpdates_UpdatesToLatestTime` | Asynchronously ensures that successive calls to `UpdateLastLogin` retain the most recent timestamp. | none | `Task` (completed when the test finishes) | Throws an assertion exception if the final `LastLogin` value is not the latest of the supplied dates. |

## Usage

```csharp
using Xunit; // or NUnit/MSTest depending on the test framework
using DotNetMicroOrm.Models; // namespace containing UserModel
using DotNetMicroOrm.Tests;  // namespace containing UserModelTests

public class ExampleTests
{
    [Fact]
    public void RunValidationTests()
    {
        var testInstance = new UserModelTests();
        // Execute a single test method; the test framework will catch any assertions.
        testInstance.Validate_WithValidUser_ReturnsTrue();
        // Additional test methods can be invoked similarly.
    }
}
```

```csharp
using System.Threading.Tasks;
using DotNetMicroOrm.Tests;

public class AsyncExample
{
    public async Task RunAsyncTest()
    {
        var testInstance = new UserModelTests();
        // Await the asynchronous test method.
        await testInstance.UpdateLastLogin_MultipleUpdates_UpdatesToLatestTime();
    }
}
```

## Notes

- Each test method is independent; they do not rely on shared static state, so they can be safely executed in parallel by the test runner.
- The methods do not accept parameters; all test data is constructed inside the method body.
- Although the signatures return `void` or `Task`, the actual validation of behavior is performed through assertions; failure of an assertion results in an exception that the test framework interprets as a test failure.
- No thread‑safety guarantees are required or provided by the test class itself; thread‑safety concerns belong to the `UserModel` implementation under test.
- The asynchronous test (`UpdateLastLogin_MultipleUpdates_UpdatesToLatestTime`) assumes that the system clock or any timing‑dependent logic inside `UserModel` is either mocked or deterministic; otherwise, the test may be flaky under heavy system load.
