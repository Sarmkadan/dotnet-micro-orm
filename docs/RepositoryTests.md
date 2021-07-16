# RepositoryTests

Unit test class for verifying the behavior of repository implementations in the dotnet-micro-orm project. It validates core CRUD operations, query capabilities, and error handling for repository classes that interact with a micro-ORM data context.

## API

### `RepositoryTests()`
Constructor for the test class. Initializes test dependencies and test context.

### `void Constructor_WithNullContext_ThrowsArgumentNullException()`
Verifies that the repository constructor throws an `ArgumentNullException` when provided with a null data context. Ensures defensive null checking in repository initialization.

### `Task GetByIdAsync_WithExistingId_ReturnsEntity()`
Tests retrieval of an entity by its unique identifier. Confirms that the repository returns the correct entity when the ID exists in the data store.

### `Task GetByIdAsync_WithNonExistingId_ReturnsNull()`
Tests retrieval behavior for non-existent IDs. Ensures the repository returns `null` when no matching entity is found, rather than throwing an exception.

### `Task GetAllAsync_WithMultipleEntities_ReturnsAllEntities()`
Validates retrieval of all entities from the repository. Confirms that the repository returns a complete list of entities when multiple records exist.

### `Task GetAllAsync_WithNoEntities_ReturnsEmptyList()`
Tests retrieval when no entities exist in the data store. Ensures the repository returns an empty collection instead of `null`.

### `Task GetAsync_WithMatchingPredicate_ReturnsFilteredEntities()`
Verifies filtered entity retrieval using a predicate. Confirms that the repository returns only entities matching the provided predicate.

### `Task GetAsync_WithNoMatchingPredicate_ReturnsEmptyList()`
Tests filtered retrieval with a predicate that matches no entities. Ensures the repository returns an empty collection when no entities satisfy the predicate.

### `Task CountAsync_WithPredicate_ReturnsCorrectCount()`
Validates counting entities that match a specific predicate. Confirms the repository returns the correct count of filtered entities.

### `Task CountAsync_WithNullPredicate_ReturnsTotalCount()`
Tests counting behavior with a null predicate. Ensures the repository returns the total number of entities when no filtering is applied.

### `Task ExistsAsync_WithExistingEntity_ReturnsTrue()`
Verifies existence checking for an existing entity. Confirms the repository returns `true` when the entity exists in the data store.

### `Task ExistsAsync_WithNonExistingEntity_ReturnsFalse()`
Tests existence checking for a non-existent entity. Ensures the repository returns `false` when the entity is not found.

### `Task AddAsync_WithValidEntity_AddsSuccessfully()`
Validates insertion of a valid entity. Confirms the repository successfully adds the entity and persists it to the data store.

### `Task AddAsync_WithInvalidEntity_ThrowsEntityValidationException()`
Tests insertion of an invalid entity. Ensures the repository throws an `EntityValidationException` when the entity fails validation rules.

### `Task UpdateAsync_WithValidEntity_UpdatesSuccessfully()`
Validates updating an existing valid entity. Confirms the repository successfully updates the entity and persists changes to the data store.

### `Task UpdateAsync_WithInvalidEntity_ThrowsEntityValidationException()`
Tests updating an invalid entity. Ensures the repository throws an `EntityValidationException` when the entity fails validation rules during update.

### `Task DeleteAsync_WithValidId_DeletesSuccessfully()`
Validates deletion of an entity by its ID. Confirms the repository successfully removes the entity from the data store.

### `Task DeleteAsync_WithNonExistingId_ReturnsFalse()`
Tests deletion of a non-existent entity by ID. Ensures the repository returns `false` when attempting to delete an entity that does not exist.

## Usage
