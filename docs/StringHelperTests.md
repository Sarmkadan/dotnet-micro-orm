# StringHelperTests

Test suite that verifies the correctness of string transformation and truncation utilities within the `dotnet-micro-orm` project. It focuses on casing conversions between kebab-case, snake_case, and PascalCase, as well as length-constrained truncation with suffix preservation.

## API

### `ToKebabCase_PascalCaseInput_ReturnsKebabCase`

Validates that a PascalCase input string is correctly converted to kebab-case format. The test supplies a PascalCase string and asserts that the output uses lowercase letters with hyphens separating words, with no leading or trailing hyphens.

- **Parameters:** None (parameterless test method).
- **Return Value:** `void`.
- **Throws:** Assertion failure if the conversion does not produce the expected kebab-case result.

### `ToSnakeCase_PascalCaseInput_ReturnsSnakeCase`

Validates that a PascalCase input string is correctly converted to snake_case format. The test supplies a PascalCase string and asserts that the output uses lowercase letters with underscores separating words, with no leading or trailing underscores.

- **Parameters:** None (parameterless test method).
- **Return Value:** `void`.
- **Throws:** Assertion failure if the conversion does not produce the expected snake_case result.

### `ToPascalCase_KebabCaseInput_ReturnsPascalCase`

Validates that a kebab-case input string is correctly converted to PascalCase format. The test supplies a kebab-case string and asserts that the output has no separators, with each word starting with an uppercase letter and the remainder in lowercase.

- **Parameters:** None (parameterless test method).
- **Return Value:** `void`.
- **Throws:** Assertion failure if the conversion does not produce the expected PascalCase result.

### `Truncate_StringExceedsMaxLength_AppendsSuffixAndRespectsBound`

Validates that truncating a string longer than the specified maximum length produces a result whose total length equals the maximum, with the provided suffix appended at the end. The test ensures the suffix is present and that the combined truncated content plus suffix does not exceed the bound.

- **Parameters:** None (parameterless test method).
- **Return Value:** `void`.
- **Throws:** Assertion failure if the truncated string exceeds the maximum length or the suffix is missing or misplaced.

## Usage

```csharp
// Verify round-trip casing conversions in a data pipeline
[TestMethod]
public void CasingRoundTrip_FromPascalToKebabAndBack_ProducesOriginal()
{
    var pascal = "OrderDetail";
    var kebab = StringHelper.ToKebabCase(pascal);
    Assert.AreEqual("order-detail", kebab);

    var backToPascal = StringHelper.ToPascalCase(kebab);
    Assert.AreEqual("OrderDetail", backToPascal);
}

// Truncate a long description for a summary field with a trailing ellipsis
[TestMethod]
public void TruncateDescription_ForPreviewDisplay_AppendsEllipsis()
{
    var description = "This is a very long product description that exceeds the preview limit significantly";
    var maxLength = 40;
    var suffix = "...";

    var truncated = StringHelper.Truncate(description, maxLength, suffix);
    Assert.AreEqual(maxLength, truncated.Length);
    Assert.IsTrue(truncated.EndsWith(suffix));
}
```

## Notes

- **Edge Cases:** The truncation test assumes the suffix length is less than the maximum length; behavior when the suffix alone exceeds the maximum is not covered by this suite and should be validated separately. Casing tests assume well-formed inputs with clear word boundaries—strings with consecutive uppercase letters, digits, or existing separators may behave differently.
- **Thread Safety:** These test methods are instance methods intended to be executed by a test runner. They do not mutate shared static state and are safe to run concurrently with other tests in the same class, provided the underlying string utility methods under test are themselves thread-safe (as expected for pure static string transformations).
