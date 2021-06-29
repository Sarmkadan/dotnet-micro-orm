# UnitOfWork

The `UnitOfWork` class implements the Unit of Work pattern for the `dotnet-micro-orm` project, providing a centralized mechanism to manage transactions, track changes, and coordinate the persistence of multiple entity operations in a single atomic operation. It serves as a lightweight abstraction over the underlying micro-ORM's change tracking and transaction handling, ensuring consistent behavior across entity repositories while minimizing boilerplate code.

## API

### `public UnitOfWork`

Initializes a new instance of the `UnitOfWork` class. This constructor prepares the unit of work context, including initializing the change tracker and transaction state. No parameters are required as the underlying ORM handles connection management internally.

### `public IRepository<T> Repository<T>() where T : BaseEntity, new()`

Creates and returns a repository instance for the specified entity type `T`, constrained to types derived from `BaseEntity`. The repository provides CRUD operations scoped to this unit of work. Each call returns the same repository instance for the given type within the same unit of work context.

- **Return value**: An `IRepository<T>` instance bound to the current unit of work.
- **Throws**: `InvalidOperationException` if the unit of work has been disposed.

### `public async Task<bool> BeginTransactionAsync()`

Begins a new database transaction asynchronously. If a transaction is already active, this method does nothing and returns `false`.

- **Return value**: `true` if a new transaction was started; `false` if one was already active.
- **Throws**: `ObjectDisposedException` if the unit of work has been disposed.

### `public async Task<bool> CommitAsync()`

Commits the current transaction asynchronously, persisting all tracked changes made through repositories obtained via this unit of work. If no transaction is active, this method does nothing and returns `false`.

- **Return value**: `true` if changes were committed; `false` if no transaction was active.
- **Throws**:
  - `ObjectDisposedException` if the unit of work has been disposed.
  - `InvalidOperationException` if the unit of work has no active transaction.

### `public async Task<bool> RollbackAsync()`

Rolls back the current transaction asynchronously, discarding all tracked changes made through repositories obtained via this unit of work. If no transaction is active, this method does nothing and returns `false`.

- **Return value**: `true` if changes were rolled back; `false` if no transaction was active.
- **Throws**: `ObjectDisposedException` if the unit of work has been disposed.

### `public async Task<int> SaveChangesAsync()`

Persists all tracked changes made through repositories obtained via this unit of work to the underlying data store asynchronously, without requiring an explicit transaction. Changes are applied immediately and are not rolled back if an error occurs during saving.

- **Return value**: The number of affected rows in the database.
- **Throws**: `ObjectDisposedException` if the unit of work has been disposed.

### `public bool HasChanges`

Gets a value indicating whether any tracked entities have been modified, added, or deleted within this unit of work.

- **Return value**: `true` if changes exist; otherwise, `false`.

### `public async ValueTask DisposeAsync()`

Asynchronously releases all resources used by the `UnitOfWork` instance, including rolling back any active transaction and clearing change tracking. This method ensures proper cleanup and should be called when the unit of work is no longer needed.

- **Return value**: A `ValueTask` representing the asynchronous cleanup operation.

## Usage

### Example 1: Basic Transaction Flow
