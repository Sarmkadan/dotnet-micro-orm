# Specification
The `Specification` type in the `dotnet-micro-orm` project is a class used to define and encapsulate query criteria for retrieving data from a database. It provides a flexible and reusable way to specify conditions, inclusions, ordering, and pagination for queries, making it easier to manage complex data retrieval logic.

## API
The `Specification` class has the following public members:
* `Criteria`: An optional expression that defines the filter criteria for the query. It is of type `Expression<Func<T, bool>>?`, where `T` is the type of the data being queried.
* `Includes`: A list of expressions that specify the related data to be included in the query results. Each expression is of type `Expression<Func<T, object>>`.
* `IncludeStrings`: A list of strings that specify the related data to be included in the query results.
* `OrderBy`: An optional expression that defines the ordering criteria for the query results. It is of type `Expression<Func<T, object>>?`.
* `OrderByDescending`: An optional expression that defines the descending ordering criteria for the query results. It is of type `Expression<Func<T, object>>?`.
* `PageNumber`: An optional integer that specifies the page number for pagination. It is of type `int?`.
* `PageSize`: An optional integer that specifies the page size for pagination. It is of type `int?`.
* `IsPagingEnabled`: A boolean that indicates whether pagination is enabled.
* `ActiveUsersSpecification`, `UserByIdSpecification`, `UsersByEmailSpecification`, `ActiveProductsSpecification`, `ProductsByPriceRangeSpecification`, `LowStockProductsSpecification`, `UserOrdersSpecification`, `PendingOrdersSpecification`, and `RecentOrdersSpecification` are predefined specifications that can be used as-is or as a starting point for custom specifications.

## Usage
Here are two examples of using the `Specification` class:
```csharp
// Example 1: Retrieving active users
var specification = new Specification<User>(u => u.IsActive);
var users = dbContext.Users.Where(specification.Criteria).ToList();

// Example 2: Retrieving products by price range
var specification = new Specification<Product>(p => p.Price >= 10 && p.Price <= 50);
specification.Includes.Add(p => p.Category);
var products = dbContext.Products.Where(specification.Criteria).Include(specification.Includes).ToList();
```

## Notes
When using the `Specification` class, note that the `Criteria` expression should be a valid lambda expression that can be translated to a database query. The `Includes` and `IncludeStrings` lists should contain valid navigation properties or related data that can be included in the query results. The `OrderBy` and `OrderByDescending` expressions should be valid lambda expressions that can be translated to a database query. The `PageNumber` and `PageSize` values should be positive integers. The `IsPagingEnabled` boolean should be set to `true` to enable pagination. The predefined specifications can be used as-is or modified to suit specific use cases. Additionally, the `Specification` class is not thread-safe, so it should not be shared across multiple threads.
