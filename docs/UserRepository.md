# UserRepository

A repository implementation for accessing and querying `User` entities in a micro-ORM context. Provides methods to retrieve users by various criteria such as username, email, status, or date ranges, and includes methods for counting active users.

## API

### `UserRepository(IDatabaseContext context)`

Constructs a new `UserRepository` instance with the provided database context.

- **Parameters**
  - `context` – An `IDatabaseContext` instance used for database operations.
- **Remarks**
  - The base repository handles connection management and transaction scope.

---

### `async Task<User?> GetByUsernameAsync(string username)`

Retrieves a user by their unique username.

- **Parameters**
  - `username` – The username to search for.
- **Returns**
  - A `User` instance if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `username` is `null` or whitespace.

---

### `async Task<User?> GetByEmailAsync(string email)`

Retrieves a user by their unique email address.

- **Parameters**
  - `email` – The email address to search for.
- **Returns**
  - A `User` instance if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `email` is `null` or whitespace.

---
### `async Task<List<User>> GetActiveUsersAsync()`

Retrieves all users marked as active.

- **Returns**
  - A list of `User` instances representing active users. May be empty.
- **Remarks**
  - Active status is determined by the `IsActive` property of the `User` entity.

---
### `async Task<List<User>> GetVerifiedUsersAsync()`

Retrieves all users who have completed email verification.

- **Returns**
  - A list of `User` instances representing verified users. May be empty.
- **Remarks**
  - Verified status is determined by the `IsVerified` property of the `User` entity.

---
### `async Task<List<User>> GetUsersByDateRangeAsync(DateTime start, DateTime end)`

Retrieves users based on a date range filter applied to their creation date.

- **Parameters**
  - `start` – The inclusive start date of the range.
  - `end` – The inclusive end date of the range.
- **Returns**
  - A list of `User` instances created within the specified date range. May be empty.
- **Exceptions**
  - Throws `ArgumentException` if `start` is after `end`.

---
### `async Task<List<User>> GetInactiveUsersAsync()`

Retrieves all users who are not currently active.

- **Returns**
  - A list of `User` instances representing inactive users. May be empty.
- **Remarks**
  - Inactive status is determined by the `IsActive` property of the `User` entity.

---
### `async Task<int> CountActiveUsersAsync()`

Counts the total number of users marked as active.

- **Returns**
  - The number of active users.
- **Remarks**
  - Active status is determined by the `IsActive` property of the `User` entity.

## Usage
