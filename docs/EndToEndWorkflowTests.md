# EndToEndWorkflowTests

The `EndToEndWorkflowTests` class contains comprehensive integration tests for the `dotnet-micro-orm` project, validating real-world scenarios across multiple components. These tests simulate full workflows, including data persistence, caching, concurrency, validation, and business logic, ensuring the system behaves as expected under various conditions. Each test method exercises a distinct use case, often combining multiple operations to verify correctness, performance, and resilience.

## API

### `CreateAndRetrieveProduct_FullWorkflow`
**Purpose**: Tests the complete lifecycle of a product, from creation to retrieval, including validation of persisted data.
**Parameters**: None.
**Return Value**: `Task` (no explicit return value; assertions are made within the method).
**Throws**: May throw exceptions if database operations fail or assertions are violated.

### `InventoryManagement_IncreaseAndDecreaseStock`
**Purpose**: Validates stock management operations, including increasing and decreasing inventory quantities, and verifies consistency.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if stock calculations or database updates fail.

### `ProfitCalculation_WithAndWithoutCost`
**Purpose**: Tests profit calculation logic, comparing scenarios with and without cost data, and ensures accurate financial computations.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if calculations or assertions fail.

### `BatchProductCreation_Simulation`
**Purpose**: Simulates bulk product creation to test performance, scalability, and correctness under load.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if batch operations fail or time out.

### `CachingStrategy_GetOrSet`
**Purpose**: Verifies caching behavior, including cache hits, misses, and invalidation, ensuring data consistency between cached and persisted states.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if caching operations fail or assertions are violated.

### `UserManagement_CompleteLifecycle`
**Purpose**: Tests the full lifecycle of user management, including creation, updates, role assignments, and deletion.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if user operations or assertions fail.

### `SpecificationPattern_ProductFiltering`
**Purpose**: Validates the specification pattern for filtering products, ensuring queries return expected results based on dynamic criteria.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if filtering logic or assertions fail.

### `ConcurrencyScenario_MultipleOperations`
**Purpose**: Tests concurrent operations to verify thread safety, isolation, and conflict resolution (e.g., optimistic concurrency).
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if concurrency conflicts or assertions fail.

### `ValidationErrorHandling_Comprehensive`
**Purpose**: Tests validation logic across multiple scenarios, ensuring invalid inputs are rejected with appropriate error messages.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if validation logic or assertions fail.

### `CacheInvalidation_PatternBased`
**Purpose**: Validates cache invalidation strategies, ensuring cached data is refreshed or removed based on predefined patterns (e.g., time-based or event-based).
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if cache invalidation or assertions fail.

### `OrderManagement_Scenario`
**Purpose**: Tests order management workflows, including creation, updates, and fulfillment, with validation of business rules.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if order operations or assertions fail.

### `DataExportScenario_MultipleFormats`
**Purpose**: Tests data export functionality across multiple formats (e.g., CSV, JSON), ensuring correctness and compatibility.
**Parameters**: None.
**Return Value**: `Task`.
**Throws**: May throw exceptions if export operations or assertions fail.

## Usage

### Example 1: Testing Product Creation and Retrieval
