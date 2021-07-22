# ProductModelTests

The `ProductModelTests` class contains unit tests for validating the behavior of a product model in the `dotnet-micro-orm` project. These tests verify the correctness of validation logic, stock management operations, and error handling for edge cases such as invalid inputs or boundary conditions.

## API

### `Validate_WithValidProduct_ReturnsTrue`
Verifies that a product with valid properties passes validation.
- **Purpose**: Ensures the `Validate` method returns `true` for a correctly configured product.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithEmptySku_ReturnsFalseWithError`
Tests validation failure when the product's SKU is an empty string.
- **Purpose**: Confirms the `Validate` method returns `false` and includes an appropriate error message.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithShortSku_ReturnsFalseWithError`
Tests validation failure when the product's SKU is shorter than the required minimum length.
- **Purpose**: Ensures the `Validate` method rejects a SKU that does not meet length requirements.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithNullSku_ReturnsFalseWithError`
Tests validation failure when the product's SKU is `null`.
- **Purpose**: Verifies the `Validate` method returns `false` and includes an error for a `null` SKU.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithEmptyName_ReturnsFalseWithError`
Tests validation failure when the product's name is an empty string.
- **Purpose**: Confirms the `Validate` method returns `false` and includes an error for an empty name.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithShortName_ReturnsFalseWithError`
Tests validation failure when the product's name is shorter than the required minimum length.
- **Purpose**: Ensures the `Validate` method rejects a name that does not meet length requirements.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithZeroPrice_ReturnsFalseWithError`
Tests validation failure when the product's price is zero.
- **Purpose**: Verifies the `Validate` method returns `false` and includes an error for a zero price.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithNegativePrice_ReturnsFalseWithError`
Tests validation failure when the product's price is negative.
- **Purpose**: Confirms the `Validate` method returns `false` and includes an error for a negative price.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithNegativeCostPrice_ReturnsFalseWithError`
Tests validation failure when the product's cost price is negative.
- **Purpose**: Ensures the `Validate` method returns `false` and includes an error for a negative cost price.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithNegativeStockQuantity_ReturnsFalseWithError`
Tests validation failure when the product's stock quantity is negative.
- **Purpose**: Verifies the `Validate` method returns `false` and includes an error for a negative stock quantity.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithZeroCategoryId_ReturnsFalseWithError`
Tests validation failure when the product's category ID is zero.
- **Purpose**: Confirms the `Validate` method returns `false` and includes an error for a zero category ID.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithNegativeCategoryId_ReturnsFalseWithError`
Tests validation failure when the product's category ID is negative.
- **Purpose**: Ensures the `Validate` method returns `false` and includes an error for a negative category ID.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `Validate_WithMultipleValidationErrors_ReturnsAllErrors`
Tests that the `Validate` method aggregates all validation errors when multiple properties are invalid.
- **Purpose**: Verifies the method returns `false` and includes all relevant error messages.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `IncreaseStock_WithPositiveQuantity_IncreasesStockCorrectly`
Tests that increasing stock with a positive quantity updates the stock level as expected.
- **Purpose**: Confirms the `IncreaseStock` method correctly increments the stock quantity.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `IncreaseStock_WithZeroQuantity_ThrowsArgumentException`
Tests that increasing stock with a zero quantity throws an `ArgumentException`.
- **Purpose**: Ensures the `IncreaseStock` method rejects a zero quantity input.
- **Parameters**: None.
- **Return Value**: None (asserts expected exception).
- **Throws**: `ArgumentException`.

### `IncreaseStock_WithNegativeQuantity_ThrowsArgumentException`
Tests that increasing stock with a negative quantity throws an `ArgumentException`.
- **Purpose**: Verifies the `IncreaseStock` method rejects a negative quantity input.
- **Parameters**: None.
- **Return Value**: None (asserts expected exception).
- **Throws**: `ArgumentException`.

### `DecreaseStock_WithValidQuantity_DecreasesStockCorrectly`
Tests that decreasing stock with a valid quantity updates the stock level as expected.
- **Purpose**: Confirms the `DecreaseStock` method correctly decrements the stock quantity.
- **Parameters**: None.
- **Return Value**: None (asserts expected outcome).
- **Throws**: None.

### `DecreaseStock_WithZeroQuantity_ThrowsArgumentException`
Tests that decreasing stock with a zero quantity throws an `ArgumentException`.
- **Purpose**: Ensures the `DecreaseStock` method rejects a zero quantity input.
- **Parameters**: None.
- **Return Value**: None (asserts expected exception).
- **Throws**: `ArgumentException`.

### `DecreaseStock_WithNegativeQuantity_ThrowsArgumentException`
Tests that decreasing stock with a negative quantity throws an `ArgumentException`.
- **Purpose**: Verifies the `DecreaseStock` method rejects a negative quantity input.
- **Parameters**: None.
- **Return Value**: None (asserts expected exception).
- **Throws**: `ArgumentException`.

### `DecreaseStock_WithMoreThanAvailable_ThrowsInvalidOperationException`
Tests that decreasing stock with a quantity exceeding available stock throws an `InvalidOperationException`.
- **Purpose**: Ensures the `DecreaseStock` method prevents stock from going negative.
- **Parameters**: None.
- **Return Value**: None (asserts expected exception).
- **Throws**: `InvalidOperationException`.

## Usage

### Example 1: Validating a Product
