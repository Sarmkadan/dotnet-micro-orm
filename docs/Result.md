# Result

The `Result` type is a discriminated union that represents either a successful outcome or a failure, eliminating the need for null checks and exception-driven control flow. It provides a functional approach to error handling across synchronous and asynchronous operations, with specialized variants for paginated results and batch operations.

## API

### `Result`

Base type for operation outcomes without a payload.

#### `public sealed record Success`

Represents a successful operation. Carries no additional data.

#### `public sealed record Failure`

Represents a failed operation. Carries no additional data.

#### `public static Result Ok`

Factory property that returns a `Success` instance.

**Returns:** `Result` — a `Success` record.

#### `public static Result Fail`

Factory property that returns a `Failure` instance.

**Returns:** `Result` — a `Failure` record.

---

### `Result<T>`

Generic result type carrying a payload of type `T` on success.

#### `public sealed record Success`

Represents a successful operation containing a value of type `T`.

#### `public sealed record Failure`

Represents a failed operation. Carries no payload.

#### `public static Result<T> Ok`

Factory property that returns a `Success` instance with a default value for `T`.

**Returns:** `Result<T>` — a `Success` record where `Value` is `default(T)`.

#### `public static Result<T> Fail`

Factory property that returns a `Failure` instance.

**Returns:** `Result<T>` — a `Failure` record.

#### `public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TOut> onFailure)`

Pattern-matches on the result, invoking the appropriate function and returning a unified output.

**Parameters:**
- `onSuccess` — function applied to the success value.
- `onFailure` — function invoked on failure.

**Returns:** `TOut` — the value produced by the matched function.

**Throws:** `ArgumentNullException` if either function is null.

#### `public Task<TOut> MatchAsync<TOut>(Func<T, Task<TOut>> onSuccess, Func<Task<TOut>> onFailure)`

Asynchronously pattern-matches on the result.

**Parameters:**
- `onSuccess` — async function applied to the success value.
- `onFailure` — async function invoked on failure.

**Returns:** `Task<TOut>` — a task producing the matched output.

**Throws:** `ArgumentNullException` if either function is null.

#### `public Result<TOut> Map<TOut>(Func<T, TOut> mapper)`

Transforms the success value using a mapping function. Returns a failure unchanged.

**Parameters:**
- `mapper` — transformation function applied to the success value.

**Returns:** `Result<TOut>` — a new success with the mapped value, or the existing failure.

**Throws:** `ArgumentNullException` if `mapper` is null.

#### `public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> asyncMapper)`

Asynchronously transforms the success value. Returns a failure unchanged.

**Parameters:**
- `asyncMapper` — async transformation function applied to the success value.

**Returns:** `Task<Result<TOut>>` — a task producing the mapped result.

**Throws:** `ArgumentNullException` if `asyncMapper` is null.

#### `public Result<T> Tap(Action<T> onSuccess)`

Executes a side-effecting action on the success value without altering the result. Returns the original result.

**Parameters:**
- `onSuccess` — action executed when the result is a success.

**Returns:** `Result<T>` — the original result instance.

**Throws:** `ArgumentNullException` if `onSuccess` is null.

#### `public async Task<Result<T>> TapAsync(Func<T, Task> asyncAction)`

Asynchronously executes a side-effecting action on the success value without altering the result.

**Parameters:**
- `asyncAction` — async action executed when the result is a success.

**Returns:** `Task<Result<T>>` — a task producing the original result instance.

**Throws:** `ArgumentNullException` if `asyncAction` is null.

#### `public override string ToString`

Returns a string representation indicating the state and, for success, the contained value.

**Returns:** `string` — e.g., `"Success(42)"` or `"Failure"`.

---

### `PagedResult<T>`

A specialized result for paginated queries, extending `Result<T>` with pagination metadata.

#### `public PagedResult<TOut> Map<TOut>(Func<T, TOut> mapper)`

Transforms the success payload while preserving pagination metadata. Returns a failure unchanged.

**Parameters:**
- `mapper` — transformation function applied to the success value.

**Returns:** `PagedResult<TOut>` — a new paged result with the mapped value and original pagination data, or the existing failure.

**Throws:** `ArgumentNullException` if `mapper` is null.

---

### `BatchOperationResult`

Represents the outcome of a batch operation, aggregating individual results.

## Usage

### Example 1: Service layer with synchronous mapping and matching

```csharp
public Result<UserDto> GetUserById(int id)
{
    var user = _repository.Find(id);
    if (user is null)
        return Result<UserDto>.Fail;

    return Result<UserDto>.Ok.Map(u => new UserDto(u.Id, u.Name));
}

public string GetUserNameOrFallback(int id)
{
    return GetUserById(id).Match(
        onSuccess: dto => dto.Name,
        onFailure: () => "Unknown User"
    );
}
```

### Example 2: Asynchronous pipeline with side effects

```csharp
public async Task<Result<OrderSummary>> ProcessOrderAsync(OrderRequest request)
{
    var validationResult = await ValidateAsync(request);

    return await validationResult
        .MapAsync(async order => await _pricingService.ApplyDiscountsAsync(order))
        .TapAsync(async summary => await _auditLogger.LogAsync(summary));
}

private async Task<Result<Order>> ValidateAsync(OrderRequest request)
{
    if (request.Items.Count == 0)
        return Result<Order>.Fail;

    var order = new Order(request.CustomerId, request.Items);
    return Result<Order>.Ok.Map(o => order);
}
```

## Notes

- **Null handling:** `Map`, `Match`, `Tap`, and their async counterparts throw `ArgumentNullException` when provided delegate arguments are null. The result instance itself is never null when obtained from the factory properties.
- **Immutability:** All result types are records and therefore immutable. `Map` and `MapAsync` produce new instances; the original result is never modified.
- **Thread safety:** Instances are immutable and safe to share across threads. The async methods do not introduce shared mutable state beyond the delegates supplied by the caller, which are the caller's responsibility to synchronize if they capture mutable data.
- **PagedResult mapping:** `PagedResult<T>.Map` preserves pagination metadata (page number, page size, total count) while transforming only the payload. If the underlying result is a failure, pagination metadata is irrelevant and the failure propagates unchanged.
- **BatchOperationResult:** Designed for scenarios where multiple operations are performed together. Its specific members follow the same success/failure discrimination pattern as the core `Result` type.
