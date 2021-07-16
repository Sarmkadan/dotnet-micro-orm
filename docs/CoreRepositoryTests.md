# CoreRepositoryTests

`CoreRepositoryTests` is a test class that verifies the functionality of a generic repository implementation in the `dotnet-micro-orm` project. It ensures that core CRUD operations, validation, and query methods behave as expected under various conditions, including edge cases such as null inputs, invalid data, and non-existent entities. The tests cover both synchronous and asynchronous operations, focusing on correctness, exception handling, and adherence to business rules.

## API

### `CoreRepositoryTests`
The test class constructor. Initializes test fixtures and dependencies required for repository testing.

---

### `Constructor_WithNullContext_ThrowsArgumentNullException`
**Purpose**: Verifies that the repository constructor throws an `ArgumentNullException` when initialized with a null database context.
**Parameters**: None.
**Return Value**: None.
**Throws**: Validates that an `ArgumentNullException` is thrown.

---

### `GetByIdAsync_WithExistingId_ReturnsProduct`
**Purpose**: Tests retrieval of an entity by its ID when the entity exists in the database.
**Parameters**: None (uses pre-seeded test data).
**Return Value**: Asserts that the returned entity matches the expected product.
**Throws**: None.

---

### `GetByIdAsync_WithNonExistingId_ReturnsNull`
**Purpose**: Ensures that retrieving an entity with a non-existent ID returns `null`.
**Parameters**: None (uses a non-existent ID).
**Return Value**: Asserts that the result is `null`.
**Throws**: None.

---

### `AddAsync_WithValidProduct_AddsSuccessfully`
**Purpose**: Validates that a valid entity is successfully added to the database.
**Parameters**: None (uses a valid product instance).
**Return Value**: Asserts that the entity is added and its properties match the expected values.
**Throws**: None.

---

### `AddAsync_WithInvalidSku_ThrowsEntityValidationException`
**Purpose**: Confirms that adding an entity with an invalid SKU (e.g., null or empty) throws an `EntityValidationException`.
**Parameters**: None (uses a product with an invalid SKU).
**Return Value**: None.
**Throws**: Validates that an `EntityValidationException` is thrown.

---

### `AddAsync_WithInvalidPrice_ThrowsEntityValidationException`
**Purpose**: Ensures that adding an entity with an invalid price (e.g., negative) throws an `EntityValidationException`.
**Parameters**: None (uses a product with an invalid price).
**Return Value**: None.
**Throws**: Validates that an `EntityValidationException` is thrown.

---

### `UpdateAsync_WithValidProduct_UpdatesSuccessfully`
**Purpose**: Tests that updating an existing entity with valid data persists the changes correctly.
**Parameters**: None (uses a valid product instance).
**Return Value**: Asserts that the entity is updated and its properties match the expected values.
**Throws**: None.

---

### `UpdateAsync_WithInvalidProduct_ThrowsEntityValidationException`
**Purpose**: Verifies that updating an entity with invalid data (e.g., null fields) throws an `EntityValidationException`.
**Parameters**: None (uses an invalid product instance).
**Return Value**: None.
**Throws**: Validates that an `EntityValidationException` is thrown.

---

### `DeleteAsync_WithExistingId_DeletesSuccessfully`
**Purpose**: Confirms that deleting an entity with an existing ID removes it from the database.
**Parameters**: None (uses an existing ID).
**Return Value**: Asserts that the deletion returns `true`.
**Throws**: None.

---

### `DeleteAsync_WithNonExistingId_ReturnsFalse`
**Purpose**: Ensures that attempting to delete a non-existent entity returns `false`.
**Parameters**: None (uses a non-existent ID).
**Return Value**: Asserts that the result is `false`.
**Throws**: None.

---

### `GetAsync_WithMatchingPredicate_ReturnsFilteredProducts`
**Purpose**: Tests that querying entities with a predicate returns only those matching the criteria.
**Parameters**: None (uses a predicate that matches test data).
**Return Value**: Asserts that the returned list contains the expected entities.
**Throws**: None.

---

### `GetAsync_WithNoMatchingPredicate_ReturnsEmptyList`
**Purpose**: Verifies that querying with a predicate that matches no entities returns an empty list.
**Parameters**: None (uses a predicate that matches no test data).
**Return Value**: Asserts that the returned list is empty.
**Throws**: None.

---

### `CountAsync_WithPredicate_ReturnsCorrectCount`
**Purpose**: Ensures that counting entities with a predicate returns the correct number of matches.
**Parameters**: None (uses a predicate that matches test data).
**Return Value**: Asserts that the count matches the expected value.
**Throws**: None.

---

### `CountAsync_WithNullPredicate_ReturnsTotalCount`
**Purpose**: Tests that counting entities with a `null` predicate returns the total number of entities in the database.
**Parameters**: None (uses a `null` predicate).
**Return Value**: Asserts that the count matches the total number of entities.
**Throws**: None.

---

### `ExistsAsync_WithExistingProduct_ReturnsTrue`
**Purpose**: Confirms that checking for the existence of an entity that exists returns `true`.
**Parameters**: None (uses an existing entity).
**Return Value**: Asserts that the result is `true`.
**Throws**: None.

---

### `ExistsAsync_WithNonExistingProduct_ReturnsFalse`
**Purpose**: Ensures that checking for the existence of a non-existent entity returns `false`.
**Parameters**: None (uses a non-existent entity).
**Return Value**: Asserts that the result is `false`.
**Throws**: None.

---

### `GetPagedAsync_WithValidParameters_ReturnsPagedResults`
**Purpose**: Tests pagination by verifying that querying with valid parameters returns the correct subset of entities.
**Parameters**: None (uses valid pagination parameters).
**Return Value**: Asserts that the returned page contains the expected entities.
**Throws**: None.

---

### `GetPagedWithCountAsync_WithValidParameters_ReturnsPagedResultsWithTotalCount`
**Purpose**: Validates that paginated queries with a total count return both the correct subset of entities and the total number of entities.
**Parameters**: None (uses valid pagination parameters).
**Return Value**: Asserts that the returned page and total count match the expected values.
**Throws**: None.

## Usage

### Example 1: Testing Entity Retrieval and Validation
