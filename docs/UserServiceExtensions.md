# UserServiceExtensions

Extension methods for `IUserService` that provide common user-related operations with asynchronous execution.

## API

### `UserExistsAsync`

Checks whether a user with the given email address exists in the system.

- **Parameters**
  - `service`: The `IUserService` instance.
  - `email`: The email address to check.
- **Return value**: `Task<bool>` – `true` if a user with the email exists; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `email` is `null`.

---

### `GetUserByEmailAsync`

Retrieves a user by their email address.

- **Parameters**
  - `service`: The `IUserService` instance.
  - `email`: The email address of the user to retrieve.
- **Return value**: `Task<User?>` – The user if found; otherwise, `null`.
- **Exceptions**: Throws `ArgumentNullException` if `email` is `null`.

---

### `UpdateEmailAsync`

Updates the email address of an existing user.

- **Parameters**
  - `service`: The `IUserService` instance.
  - `userId`: The unique identifier of the user.
  - `newEmail`: The new email address to assign.
- **Return value**: `Task<User>` – The updated user entity.
- **Exceptions**
  - Throws `ArgumentNullException` if `newEmail` is `null`.
  - Throws `ArgumentException` if `userId` is invalid or `newEmail` is malformed.

---

### `GetActiveUsersAsync`

Retrieves all users marked as active in the system.

- **Parameters**
  - `service`: The `IUserService` instance.
- **Return value**: `Task<List<User>>` – A list of active users. Never `null`; may be empty.
- **Exceptions**: None.

---
### `AuthenticateWithDetailsAsync`

Attempts to authenticate a user using email and password, returning the user and success status.

- **Parameters**
  - `service`: The `IUserService` instance.
  - `email`: The email address of the user.
  - `password`: The plaintext password to verify.
- **Return value**: `Task<(User? User, bool Success)>` – A tuple where `User` is the authenticated user if successful (`Success == true`); otherwise, `null`. `Success` indicates whether authentication succeeded.
- **Exceptions**
  - Throws `ArgumentNullException` if `email` or `password` is `null`.
  - Throws `ArgumentException` if `email` is malformed or `password` is empty.

---
### `HasNotLoggedInDaysAsync`

Determines whether a user has not logged in for a specified number of days.

- **Parameters**
  - `service`: The `IUserService` instance.
  - `userId`: The unique identifier of the user.
  - `days`: The number of days of inactivity to check.
- **Return value**: `Task<bool>` – `true` if the user has not logged in within the given `days`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `days` is negative.
  - Throws `ArgumentException` if `userId` is invalid.

---
### `GetUserStatisticsAsync`

Retrieves counts of active, inactive, and total users in the system.

- **Parameters**
  - `service`: The `IUserService` instance.
- **Return value**: `Task<(int ActiveCount, int InactiveCount, int TotalCount)>` – A tuple containing counts of active, inactive, and total users. All values are non-negative.
- **Exceptions**: None.

---
### `GetUsersByUsernamePatternAsync`

Retrieves users whose usernames match a given pattern (case-insensitive, supports `%` wildcards).

- **Parameters**
  - `service`: The `IUserService` instance.
  - `pattern`: The search pattern (e.g., `"john%"`).
- **Return value**: `Task<List<User>>` – A list of matching users. Never `null`; may be empty.
- **Exceptions**: Throws `ArgumentNullException` if `pattern` is `null`.

---

## Usage

### Example 1: Authenticating and retrieving a user
