# AuthenticationMiddleware

Middleware component for authenticating API requests using API keys. It integrates with the `AuthorizationMiddleware` to enforce role-based access control on operations. The middleware maintains an in-memory registry of valid API keys and their associated roles, allowing dynamic key management during runtime.

## API

### `AuthenticationMiddleware`

Constructor for the middleware. Initializes a new instance with an optional initial set of API keys and roles.

### `Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the authentication middleware to validate the API key from the request and attach the associated roles to the context for downstream authorization.

- **Parameters**
  - `context` – The `HttpContext` containing the incoming request.
  - `next` – The delegate representing the next middleware in the pipeline.
- **Return value** – A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` or `next` is `null`.
  - Throws `UnauthorizedAccessException` if the API key is missing or invalid.

### `void RegisterApiKey(string apiKey, IEnumerable<string> roles)`

Registers a new API key with the specified roles. The key becomes immediately valid for authentication.

- **Parameters**
  - `apiKey` – The API key to register (must not be `null` or whitespace).
  - `roles` – The collection of roles associated with the key (must not be `null`).
- **Exceptions**
  - Throws `ArgumentNullException` if `apiKey` or `roles` is `null`.
  - Throws `ArgumentException` if `apiKey` is empty or contains only whitespace.

### `void RevokeApiKey(string apiKey)`

Removes the specified API key from the registry, invalidating it for future requests.

- **Parameters**
  - `apiKey` – The API key to revoke.
- **Exceptions**
  - Throws `ArgumentNullException` if `apiKey` is `null`.
  - Throws `KeyNotFoundException` if the key does not exist.

### `static bool HasRole(HttpContext context, string role)`

Checks whether the authenticated API key in the context possesses the specified role.

- **Parameters**
  - `context` – The `HttpContext` containing the request and attached roles.
  - `role` – The role to check (must not be `null`).
- **Return value** – `true` if the role is present; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` or `role` is `null`.

### `static bool HasAnyRole(HttpContext context, IEnumerable<string> roles)`

Checks whether the authenticated API key in the context possesses any of the specified roles.

- **Parameters**
  - `context` – The `HttpContext` containing the request and attached roles.
  - `roles` – The collection of roles to check (must not be `null`).
- **Return value** – `true` if any role is present; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` or `roles` is `null`.

## Usage

### Basic Setup in ASP.NET Core Pipeline
