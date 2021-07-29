# RepositoryExtensions

RepositoryExtensions provides a set of asynchronous extension methods for the `Repository<T>` type, enabling common data‑access operations such as fetching entities by identifier, retrieving the first entity, and obtaining paged results with total counts.

## API

### GetByIdsAsync<T>
```csharp
public static async Task<List<T>> GetByIdsAsync<T>(this Repository<T> repository, IEnumerable<int> ids) where T : BaseEntity, new
```
**Purpose**  
Asynchronously returns a list of entities of type `T` whose identifiers are present in the supplied `ids` collection.

**Parameters**  
- `repository`: The `Repository<T>` instance on which the method is invoked.  
- `ids`: An `IEnumerable<int>` containing the identifiers to look up.

**Return value**  
A `Task<List<T>>` that completes with the matching entities. The list may be empty if no identifiers match; ordering is not guaranteed.

**Exceptions**  
- `ArgumentNullException` if `repository` is `null`.  
- `ArgumentNullException` if `ids` is `null`.

### FirstOrDefaultAsync<T>
```csharp
public static async Task<T?> FirstOrDefaultAsync<T>(this Repository<T> repository)
```
**Purpose**  
Asynchronously returns the first entity of type `T` from the repository, or `null` if the repository contains no entities.

**Parameters**  
- `repository`: The `Repository<T>` instance on which the method is invoked.

**Return value**  
A `Task<T?>` that completes with the first entity or `null`.

**Exceptions**  
- `ArgumentNullException` if `repository` is `null`.

### GetPagedAsync<T>
```csharp
public static async Task<(List<T> Items, int TotalCount)> GetPagedAsync<T>(this Repository<T> repository)
```
**Purpose**  
Asynchronously retrieves a page of entities together with the total count of entities available in the repository.

**Parameters**  
- `repository`: The `Repository<T>` instance on which the method is invoked.

**Return value**  
A `Task<(List<T> Items, int TotalCount)>` where `Items` is the page of entities and `TotalCount` is the total number of entities in the repository.

**Exceptions**  
- `ArgumentNullException` if `repository` is `null`.

### GetPagedWithSkipAsync<T>
```csharp
public static async Task<(List<T> Items, int TotalCount)> GetPagedWithSkipAsync<T>(this Repository<T> repository)
```
**Purpose**  
Asynchronously retrieves a page of entities using a skip‑based approach, together with the total count of entities available.

**Parameters**  
- `repository`: The `Repository<T>` instance on which the method is invoked.

**Return value**  
A `Task<(List<T> Items, int TotalCount)>` where `Items` is the skipped page of entities and `TotalCount` is the total number of entities in the repository.

**Exceptions**  
- `ArgumentNullException` if `repository` is `null`.

## Usage

### Example 1: Fetching entities by identifiers
```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using YourNamespace.Data;      // Repository<T>
using YourNamespace.Extensions; // RepositoryExtensions

public async Task<Order[]> GetOrdersByIdsAsync(IEnumerable<int> orderIds)
{
    var repository = new Repository<Order>();
    // Returns a list of Order objects whose Id values are in orderIds.
    List<Order> orders = await repository.GetByIdsAsync(orderIds);
    return orders.ToArray();
}
```

### Example 2: Retrieving a paged result set
```csharp
using System.Threading.Tasks;
using YourNamespace.Data;
using YourNamespace.Extensions;

public async Task<(List<Product> Items, int TotalCount)> GetProductPageAsync()
{
    var repository = new Repository<Product>();
    // Returns a tuple containing the current page of products and the total count.
    return await repository.GetPagedAsync();
}
```

## Notes

- The extension methods themselves contain no state; thread safety is determined by the underlying `Repository<T>` instance. If the repository is thread‑safe, concurrent calls are safe; otherwise callers must synchronize access.
- Passing `null` for the `repository` argument results in an `ArgumentNullException`.
- For `GetByIdsAsync`, supplying an empty `ids` enumeration yields an empty list; supplying `null` throws `ArgumentNullException`.
- `FirstOrDefaultAsync` does not accept a predicate; it returns the first entity according to the repository’s internal ordering, which may be undefined if the repository does not enforce ordering.
- `GetPagedAsync` and `GetPagedWithSkipAsync` rely on the repository’s default page size and skip values; consult the repository implementation for specifics.
- The `List<T>` returned inside the tuple is newly allocated on each call. Mutating the list may affect subsequent operations if the repository expects the list to remain unchanged.
- These methods are constrained to entities that derive from `BaseEntity` and possess a parameterless constructor (as required by `GetByIdsAsync`). Other repository methods may have different constraints.
