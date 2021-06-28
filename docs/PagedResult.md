# PagedResult

A generic container for paginated query results, providing both the subset of items for the current page and metadata about the total result set and pagination state.

## API

### Properties

- **`Items`** (type: `List<T>`)
  The subset of items returned for the current page. Never `null`; an empty list is returned when no items match the query.

- **`TotalCount`** (type: `int`)
  The total number of items matching the query across all pages. Used to compute the total number of pages.

- **`PageNumber`** (type: `int`)
  One-based index of the current page (e.g., `1` for the first page). Must be positive.

- **`PageSize`** (type: `int`)
  Maximum number of items to return per page. Must be positive.

- **`OrderBy`** (type: `string?`)
  Name of the property used to sort the results. `null` indicates no explicit ordering.

- **`SortDirection`** (type: `string`)
  Direction of the sort applied: `"Ascending"` or `"Descending"`. Defaults to `"Ascending"` when not specified.

### Methods

- **`Create<T>(items, totalCount, pageNumber, pageSize, orderBy, sortDirection)`** (type: `static`)
  Constructs a new `PagedResult<T>` with the provided values.
  **Parameters:**
  - `items`: The list of items for the current page.
  - `totalCount`: Total number of items across all pages.
  - `pageNumber`: One-based index of the current page.
  - `pageSize`: Number of items per page.
  - `orderBy`: Optional property name used for sorting.
  - `sortDirection`: Direction of the sort (`"Ascending"` or `"Descending"`).
  **Returns:** A new `PagedResult<T>` instance.
  **Throws:** `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is not positive, or if `sortDirection` is invalid.

- **`FromQueryable<T>(query, pageNumber, pageSize, orderBy, sortDirection)`** (type: `static`)
  Executes the given `IQueryable<T>` and returns a paginated result.
  **Parameters:**
  - `query`: The query to execute.
  - `pageNumber`: One-based index of the current page.
  - `pageSize`: Number of items per page.
  - `orderBy`: Optional property name used for sorting.
  - `sortDirection`: Direction of the sort (`"Ascending"` or `"Descending"`).
  **Returns:** A `PagedResult<T>` containing the paged items and total count.
  **Throws:** `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is not positive, or if `sortDirection` is invalid.

- **`Map<TNew>(selector)`** (type: `PagedResult<TNew>`)
  Projects each item in `Items` using the provided selector and returns a new `PagedResult<TNew>` with the same pagination metadata.
  **Parameters:**
  - `selector`: Function to transform each item.
  **Returns:** A new `PagedResult<TNew>` with transformed items and unchanged metadata.

- **`GetNextPageInfo()`** (type: `PaginationInfo`)
  Computes pagination metadata for the next page, if one exists.
  **Returns:** A `PaginationInfo` describing the next page, or `null` if no next page exists.

- **`GetPreviousPageInfo()`** (type: `PaginationInfo`)
  Computes pagination metadata for the previous page, if one exists.
  **Returns:** A `PaginationInfo` describing the previous page, or `null` if no previous page exists.

- **`GetSkip()`** (type: `int`)
  Computes the number of items to skip to reach the first item of the current page.
  **Returns:** Zero-based offset for the current page.

- **`GetTake()`** (type: `int`)
  Returns the number of items to take for the current page, bounded by `PageSize`.

### Nested Type

- **`PaginationInfo`**
  Immutable descriptor for a page, containing `Page`, `PageSize`, `OrderBy`, and `SortDirection`.

  - **`Create(page, pageSize, orderBy, sortDirection)`** (type: `static`)
    Constructs a new `PaginationInfo`.
    **Parameters:** Same as `PagedResult.Create`.
    **Returns:** A new `PaginationInfo`.
    **Throws:** `ArgumentOutOfRangeException` if `page` or `pageSize` is not positive, or if `sortDirection` is invalid.

  - **`Page`** (type: `int`)
    One-based index of the page.

  - **`PageSize`** (type: `int`)
    Number of items per page.

## Usage

### Basic Pagination
