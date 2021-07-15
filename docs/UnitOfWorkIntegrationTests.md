# UnitOfWorkIntegrationTests

Integration tests for the `UnitOfWork` class in the `dotnet-micro-orm` project, validating transaction management, repository scoping, and resource cleanup behavior under realistic database conditions.

## API

### `UnitOfWorkIntegrationTests`
Constructor for the test fixture. Initializes the test environment with a fresh database context for each test run.

### `Constructor_WithNullContext_ThrowsArgumentNullException`
Verifies that the `UnitOfWork` constructor throws an `ArgumentNullException` when provided with a null database context.

### `Repository_WithEntityType_ReturnsRepositoryInstance`
Ensures that calling `Repository<T>()` on a `UnitOfWork` instance returns a valid repository instance for the specified entity type.

### `Repository_WithSameEntityType_ReturnsSameInstance`
Confirms that repeated calls to `Repository<T>()` with the same entity type return the same repository instance, enforcing singleton behavior per entity type within a unit of work.

### `Repository_WithDifferentEntityTypes_ReturnsDifferentInstances`
Validates that calls to `Repository<T>()` with different entity types return distinct repository instances, ensuring proper isolation between entity types.

### `BeginTransactionAsync_WithoutActiveTransaction_Succeeds`
Tests that `BeginTransactionAsync()` succeeds when no transaction is currently active, establishing a new transaction scope.

### `BeginTransactionAsync_WithActiveTransaction_ThrowsOrmException`
Checks that invoking `BeginTransactionAsync()` while a transaction is already active results in an `OrmException`, preventing nested transactions.

### `BeginTransactionAsync_WithFailure_ReturnsFalse`
Ensures that `BeginTransactionAsync()` returns `false` when the underlying database operation fails, allowing the caller to handle the failure gracefully.

### `CommitAsync_WithoutActiveTransaction_ThrowsOrmException`
Verifies that `CommitAsync()` throws an `OrmException` when no transaction is active, indicating an invalid state for committing.

### `CommitAsync_WithActiveTransaction_Succeeds`
Confirms that `CommitAsync()` successfully persists all pending changes when a transaction is active and the commit operation succeeds.

### `CommitAsync_WithDatabaseFailure_RollsbackAndThrows`
Validates that `CommitAsync()` rolls back all changes and throws an `OrmException` when the database operation fails during commit.

### `RollbackAsync_WithoutActiveTransaction_ReturnsTrue`
Ensures that `RollbackAsync()` returns `true` when no transaction is active, indicating that no rollback was necessary.

### `RollbackAsync_WithActiveTransaction_Succeeds`
Tests that `RollbackAsync()` successfully undoes all pending changes when a transaction is active.

### `RollbackAsync_WithDatabaseFailure_ThrowsOrmException`
Verifies that `RollbackAsync()` throws an `OrmException` when the database operation fails during rollback.

### `SaveChangesAsync_WithoutChanges_ReturnsZero`
Checks that `SaveChangesAsync()` returns `0` when no changes are pending, indicating no work was performed.

### `Dispose_CleansUpResources`
Ensures that calling `Dispose()` on a `UnitOfWork` instance properly releases all held resources, including database connections and transactions.

### `MultipleRepositories_MaintainIndependentState`
Validates that multiple repositories obtained from the same `UnitOfWork` operate on independent state until changes are committed or rolled back.

### `TransactionWorkflow_BeginCommitSequence`
Tests a complete transaction workflow: beginning a transaction, performing operations, committing, and verifying persistence.

### `TransactionWorkflow_BeginRollbackSequence`
Tests a complete transaction workflow: beginning a transaction, performing operations, rolling back, and verifying that changes are discarded.

## Usage

### Basic Transaction Flow
