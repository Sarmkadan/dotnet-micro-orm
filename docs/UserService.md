# UserService

`UserService` manages the lifecycle of user accounts within the `dotnet-micro-orm` project, providing asynchronous operations for registration, authentication, profile management, password changes, email verification, and administrative tasks such as deactivation and user counting. It implements `IAsyncDisposable` to release underlying database resources cleanly.

## API

### `public UserService`
Constructor. Initializes a new instance of the service, typically accepting an injected database connection or repository dependency. The exact parameter list is determined by the underlying implementation but is not exposed as a public overload in the documented surface.

### `public async Task<User> RegisterUserAsync`
Registers a new user in the system.

- **Parameters:** Accepts a registration model containing at minimum a username/email and password (exact parameter type inferred from usage).
- **Returns:** The newly created `User` entity with its assigned identifier.
- **Throws:** `ArgumentException` when the username or email is already taken; `ValidationException` when required fields are missing or the password does not meet complexity requirements.

### `public async Task<User?> AuthenticateAsync`
Validates user credentials and returns the authenticated user.

- **Parameters:** Credentials object containing a login identifier (username or email) and a plain-text password.
- **Returns:** The matching `User` if authentication succeeds; `null` if credentials are invalid or the account is deactivated.
- **Throws:** `InvalidOperationException` when the underlying data store is unreachable.

### `public async Task<User?> GetUserByIdAsync`
Retrieves a user by their unique identifier.

- **Parameters:** `int` or `Guid` user ID.
- **Returns:** The `User` entity if found; `null` if no user exists with the given ID.
- **Throws:** No exceptions under normal conditions; database-level exceptions propagate if connectivity is lost.

### `public async Task<User> UpdateProfileAsync`
Updates mutable profile fields for an existing user (e.g., display name, avatar URL, timezone).

- **Parameters:** A user ID and a profile update model containing the fields to change.
- **Returns:** The updated `User` entity reflecting all persisted changes.
- **Throws:** `KeyNotFoundException` when the specified user ID does not exist; `ValidationException` when updated fields violate constraints (e.g., display name exceeds maximum length).

### `public async Task<bool> ChangePasswordAsync`
Changes the password for an authenticated user after verifying the current password.

- **Parameters:** User ID, current plain-text password, and new plain-text password.
- **Returns:** `true` if the password was successfully changed; `false` if the current password is incorrect.
- **Throws:** `ArgumentException` when the new password fails complexity requirements; `KeyNotFoundException` when the user ID does not exist.

### `public async Task<bool> VerifyEmailAsync`
Marks a user’s email address as verified, typically triggered by a confirmation token.

- **Parameters:** User ID and a verification token string.
- **Returns:** `true` if the token is valid and the email is successfully marked verified; `false` if the token is expired, invalid, or already consumed.
- **Throws:** `KeyNotFoundException` when the user ID does not exist.

### `public async Task<int> GetActiveUsersCountAsync`
Returns the total number of users currently in an active (non-deactivated) state.

- **Parameters:** None.
- **Returns:** A non-negative integer representing the count of active users.
- **Throws:** Database-level exceptions propagate if the query fails.

### `public async Task<List<User>> GetInactiveUsersAsync`
Retrieves all users whose accounts have been deactivated.

- **Parameters:** None.
- **Returns:** A list of `User` entities; an empty list if no inactive users exist.
- **Throws:** Database-level exceptions propagate if the query fails.

### `public async Task<bool> DeactivateUserAsync`
Deactivates a user account, preventing authentication and marking the account as inactive.

- **Parameters:** User ID.
- **Returns:** `true` if the user was successfully deactivated; `false` if the user was already inactive.
- **Throws:** `KeyNotFoundException` when the user ID does not exist.

### `public async ValueTask DisposeAsync`
Asynchronously releases all resources held by the service instance, such as database connections or pooled objects. After disposal, any further calls to service methods will throw `ObjectDisposedException`.

## Usage

### Example 1: Registration and Authentication Flow

```csharp
await using var userService = new UserService(connectionFactory);

// Register a new user
var newUser = await userService.RegisterUserAsync(new RegistrationModel
{
    Email = "alice@example.com",
    Password = "Str0ng!Pass",
    DisplayName = "Alice"
});

// Later, authenticate the user
var authenticatedUser = await userService.AuthenticateAsync(new Credentials
{
    Login = "alice@example.com",
    Password = "Str0ng!Pass"
});

if (authenticatedUser is not null)
{
    Console.WriteLine($"Welcome back, {authenticatedUser.DisplayName}");
}
```

### Example 2: Administrative Cleanup of Inactive Accounts

```csharp
await using var userService = new UserService(connectionFactory);

var inactiveUsers = await userService.GetInactiveUsersAsync();
Console.WriteLine($"Found {inactiveUsers.Count} inactive accounts.");

foreach (var user in inactiveUsers)
{
    // Attempt to deactivate again (idempotent check)
    bool wasDeactivated = await userService.DeactivateUserAsync(user.Id);
    Console.WriteLine($"User {user.Id} deactivated this run: {wasDeactivated}");
}

int activeCount = await userService.GetActiveUsersCountAsync();
Console.WriteLine($"Remaining active users: {activeCount}");
```

## Notes

- **Deactivated accounts and authentication:** `AuthenticateAsync` returns `null` for deactivated users even if the credentials are correct. The caller cannot distinguish between a wrong password and a deactivated account solely from the return value.
- **Idempotency of `DeactivateUserAsync`:** Calling `DeactivateUserAsync` on an already inactive user returns `false` without throwing. This allows safe batch operations without pre-checking status.
- **Email verification tokens:** `VerifyEmailAsync` expects the exact token issued during registration. Tokens are one-time-use; a second call with the same token returns `false`.
- **Password change atomicity:** `ChangePasswordAsync` verifies the current password and updates to the new password in a single database transaction. If the new password fails validation, the transaction is rolled back and the old password remains intact.
- **Thread safety:** This service is not designed for concurrent use on the same instance. Each request should operate on its own scoped instance, typically managed via dependency injection with a scoped lifetime. Shared state is limited to the injected database connection, which itself must be used sequentially.
- **Disposal:** Always dispose the service via `await using` or explicit `DisposeAsync` to return database connections to the pool. Failure to dispose may lead to connection leaks under load.
