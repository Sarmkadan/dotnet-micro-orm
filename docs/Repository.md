# Repository
Repository<T> provides an asynchronous abstraction for performing CRUD and query operations on entities that inherit from BaseEntity. It encapsulates data‑access logic, allowing consumers to work with strongly‑typed collections without exposing the underlying persistence mechanism.

## API
- **public Repository()**  
  Initializes a new instance of the Repository<T> class. The underlying data source must be configured before any operation is invoked (e.g., via dependency injection or setting a DbContext).

- **public async Task<T?> GetByIdAsync(object id)**  
  Retrieves a single entity by its primary key.  
  *Parameters:* `id` – the key value; must not be null.  
  *Returns:* The entity if found, otherwise null.  
  *Throws:* `ArgumentNullException` if `id` is null; `InvalidOperationException` if the repository is not initialized; any data‑source specific exception on query failure.

- **public async Task<T?> FirstOrDefaultAsync()**  
  Returns the first entity in the collection or null if the collection is empty.  
  *Parameters:* none.  
  *Returns:* The first entity or null.  
  *Throws:* `InvalidOperationException` if the repository is not initialized; propagates query‑related exceptions.

- **public async Task<List<T>> GetAllAsync()**  
  Retrieves all entities of type T.  
  *Parameters:* none.  
  *Returns:* A list containing all entities; may be empty.  
  *Throws:* `InvalidOperationException` if the repository is not initialized; any exception from the data source.

- **public async Task<List<T>> GetAsync()**  
  Retrieves entities based on the current query state (see `Query`).  
  *Parameters:* none.  
  *Returns:* A list of entities matching the query; may be empty.  
  *Throws:* `InvalidOperationException` if the repository is not initialized; query‑related exceptions.

- **public async Task<int> CountAsync()**  
  Gets the total number of entities in the collection.  
  *Parameters:* none.  
  *Returns:* The count as `Throws: `InvalidOperationException` if the repository is not initialized; data‑source exceptions.

- **public async Task<bool> ExistsAsync()**  
  Determines whether any entity exists in the collection.  
  *Parameters:* none.  
  *Returns:* `true` if at least one entity is present; otherwise `false`.  
  *Throws:* `InvalidOperationException` if the repository is not initialized; propagates exceptions from the underlying store.

- **public async Task<T> AddAsync(T entity)**  
  Inserts a new entity into the repository.  
  *Parameters:* `entity` – the instance to add; must not be null and must be a valid BaseEntity.  
  *Returns:* The added entity, typically with generated key values populated.  
  *Throws:* `ArgumentNullException` if `entity` is null; `InvalidOperationException` if the repository is not initialized; validation or persistence exceptions.

- **public async Task<T> UpdateAsync(T entity)**  
  Updates an existing entity.  
  *Parameters:* `entity` – the instance with modified values; must not be null and must be tracked by the repository.  
  *Returns:* The updated entity.  
  *Throws:* `ArgumentNullException` if `entity` is null; `InvalidOperationException` if the entity is not tracked or the repository is not initialized; persistence exceptions.

- **public async Task<bool> DeleteAsync(T entity)** *(overload 1)*  
  Marks the specified entity for deletion.  
  *Parameters:* `entity` – the instance to remove; must not be null.  
  *Returns:* `true` if the entity was successfully queued for deletion; otherwise `false`.  
  *Throws:* `ArgumentNullException` if `entity` is null; `InvalidOperationException` if the repository is not initialized; data‑source exceptions.

- **public async Task<bool> DeleteAsync(T entity)** *(overload 2)*  
  Alternate overload for deleting an entity (behaviour identical to overload 1).  
  *Parameters:* `entity` – the instance to remove; must not be null.  
  *Returns:* `true` on success, `false` otherwise.  
  *Throws:* Same as overload 1.

- **public async Task<List<T>> AddRangeAsync(IEnumerable<T> entities)**  
  Adds a collection of entities.  
  *Parameters:* `entities` – sequence of instances to add; must not be null.  
  *Returns:* List of the added entities with any generated values.  
  *Throws:* `ArgumentNullException` if `entities` is null; `InvalidOperationException` if the repository is not initialized; validation or persistence exceptions.

- **public async Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities)**  
  Updates a collection of entities.  
  *Parameters:* `entities` – sequence of instances to update; must not be null and each must be tracked.  
  *Returns:* List of the updated entities.  
  *Throws:* `ArgumentNullException` if `entities` is null; `InvalidOperationException` if any entity is not tracked or the repository is not initialized; persistence exceptions.

- **public async Task<int> DeleteRangeAsync(IEnumerable<T> entities)**  
  Deletes a collection of entities.  
  *Parameters:* `entities` – sequence of instances to remove; must not be null.  
  *Returns:* The number of entities that were queued for deletion.  
  *Throws:* `ArgumentNullException` if `entities` is null; `InvalidOperationException` if the repository is not initialized; data‑source exceptions.

- **public async Task<List<T>> GetPagedAsync(int pageIndex, int pageSize)**  
  Retrieves a page of entities.  
  *Parameters:* `pageIndex` – zero‑based index of the page; `pageSize` – maximum number of items per page (must be > 0).  
  *Returns:* List of entities for the requested page; may be empty.  
  *Throws:* `ArgumentOutOfRangeException` if `pageIndex` < 0 or `pageSize` ≤ 0; `InvalidOperationException` if the repository is not initialized; query‑related exceptions.

- **public async Task<(List<T> Items, int TotalCount)> GetPagedWithCountAsync(int pageIndex, int pageSize)**  
  Retrieves a page of entities together with the total count.  
  *Parameters:* `pageIndex` – zero‑based page index; `pageSize` – page size (> 0).  
  *Returns:* A tuple where `Items` is the list of entities for the page and `TotalCount` is the total number of entities matching the query.  
  *Throws:* Same as `GetPagedAsync`.

- **public IQueryable<T> Query**  
  Exposes the underlying queryable for further LINQ composition before execution.  
  *Returns:* An `IQueryable<T>` representing the current query.  
  *Throws:* `InvalidOperationException` if the repository is not initialized.

- **public async IAsyncEnumerable<T> QueryStreamAsync()**  
  Provides an asynchronous stream of entities, useful for large result sets.  
  *Parameters:* none.  
  *Returns:* An `IAsyncEnumerable<T>` that yields entities as they are retrieved.  
  *Throws:* `InvalidOperationException` if the repository is not initialized; enumeration‑time exceptions from the data source.

- **public string? PropertyName { get; set; }**  
  Optional property name used by certain query implementations (e.g., for ordering or filtering).  
  *Get:* Returns the currently set property name or null if none is defined.  
  *Set:* Assigns a property name; setting to null clears any previous value.  
  *Throws:* No exceptions are thrown by the property itself; misuse may lead to query errors downstream.

## Usage
```csharp
// Example 1: Retrieve a single entity by ID
var repo = new Repository<Order>();
Order? order = await repo.GetByIdAsync(42);
if (order != null)
{
    Console.WriteLine($"Order {order.Id} total: {order.Total}");
}
```

```csharp
// Example 2: Paged retrieval with total count
var repo = new Repository<Product>();
var (page, total) = await repo.GetPagedWithCountAsync(pageIndex: 2, pageSize: 25);
Console.WritePage($"Showing {page.Count} of {total} products.");
foreach (var p in page)
{
    Console.WriteLine(p.Name);
}
```

## Notes
- All asynchronous methods will throw `InvalidOperationException` if the underlying data source has not been supplied before the call.
- The repository is **not thread‑safe**; concurrent calls from multiple threads on the same instance may result in undefined behavior. For multi‑threaded scenarios, create a separate repository instance per thread or synchronize access.
- `PropertyName` does not affect the core CRUD operations; it is consulted only by query‑building extensions that rely on it (e.g., dynamic ordering). Setting it to an invalid property name may cause runtime exceptions when the query is executed.
- The two `DeleteAsync` overloads share the same signature; they exist to allow different calling conventions (e.g., one may be invoked via an interface method, the other directly). Their behaviour is identical as described.
- Methods returning collections (`GetAllAsync`, `GetAsync`, `GetPagedAsync`, `AddRangeAsync`, `UpdateRangeAsync`) return an empty list when no data matches the criteria rather than null.
- `AddAsync` and `UpdateAsync` return the entity instance after the operation, which may contain updated values such as database‑generated keys or concurrency tokens.
- `DeleteRangeAsync` returns the number of entities marked for deletion; this may differ from the number actually committed if the transaction is later rolled back.
