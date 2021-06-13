# AuditLog

The `AuditLog` type is a data model used to record and track changes, actions, and system events within applications built with dotnet-micro-orm. It captures metadata about operations such as inserts, updates, and deletions, including contextual information like the user responsible, timestamps, and the nature of the change. This type is typically used to maintain an immutable history of significant actions for auditing, debugging, and compliance purposes.

## API

### Properties

- **`public int Id`**
  A unique identifier for the audit log entry. Assigned automatically when the entry is persisted.

- **`public string EntityType`**
  The fully qualified type name of the entity involved in the logged action (e.g., `MyApp.Models.Product`). Used to distinguish between different entity types in the audit trail.

- **`public int EntityId`**
  The identifier of the specific entity instance that was affected by the action. Combined with `EntityType`, uniquely identifies the target of the operation.

- **`public string Action`**
  A short string describing the type of action performed (e.g., `"Insert"`, `"Update"`, `"Delete"`). Indicates the nature of the operation being audited.

- **`public int? UserId`**
  The identifier of the user who initiated the action, if applicable. May be `null` for system-generated or anonymous actions.

- **`public string? Username`**
  The username of the user who initiated the action, if available. May be `null` for system or unauthenticated actions.

- **`public string? OldValues`**
  A serialized representation of the entity’s state before the action was applied. Format and content depend on the serialization strategy used by the application. May be `null` for insert actions.

- **`public string? NewValues`**
  A serialized representation of the entity’s state after the action was applied. Format and content depend on the serialization strategy used by the application. May be `null` for delete actions.

- **`public string? ChangedProperties`**
  A serialized list or summary of the specific properties that were modified during an update action. May be `null` if no properties were changed or if the action was not an update.

- **`public string? IPAddress`**
  The IP address from which the action was initiated, if available. Used for security and access tracking.

- **`public string? UserAgent`**
  The user agent string of the client that initiated the action, if available. Useful for identifying the source of the request (e.g., browser, mobile app).

- **`public string? Description`**
  A human-readable description or comment explaining the purpose or context of the action. Optional and application-defined.

- **`public bool IsSuccessful`**
  Indicates whether the action completed successfully. `true` if the operation succeeded; `false` otherwise.

- **`public string? ErrorMessage`**
  Contains the error message if the action failed (`IsSuccessful == false`). Otherwise, `null`.

- **`public DateTime Timestamp`**
  The UTC date and time when the action occurred. Set automatically when the log entry is created.

### Constructors

- **`public AuditLog()`**
  Initializes a new instance of the `AuditLog` class with default values. All properties except `Timestamp` will be set to their default states (`null`, `0`, `false`, etc.).

- **`public AuditLog`**
  *(Note: This appears to be a typo in the provided signature. Assuming it refers to the parameterless constructor above.)*

### Methods

- **`public override bool Validate()`**
  Validates the current state of the `AuditLog` instance. Returns `true` if all required fields are set appropriately (e.g., `EntityType`, `EntityId`, `Action`, `Timestamp` are non-null or valid); otherwise, returns `false`. Does not throw exceptions. The exact validation rules are implementation-defined.

- **`public static AuditLog CreateInsert(object entity, int userId, string? username)`**
  Creates a new `AuditLog` entry for an insert operation. Parameters:
  - `entity`: The entity being inserted.
  - `userId`: The ID of the user performing the action.
  - `username`: The username of the user, if available.
  Returns a populated `AuditLog` instance with `Action` set to `"Insert"`, `OldValues` set to `null`, and `NewValues` containing the serialized entity. May throw `ArgumentNullException` if `entity` is `null`.

- **`public static AuditLog CreateUpdate(object entity, object originalEntity, int userId, string? username)`**
  Creates a new `AuditLog` entry for an update operation. Parameters:
  - `entity`: The updated entity.
  - `originalEntity`: The entity state before the update.
  - `userId`: The ID of the user performing the action.
  - `username`: The username of the user, if available.
  Returns a populated `AuditLog` instance with `Action` set to `"Update"`, `OldValues` containing the serialized `originalEntity`, `NewValues` containing the serialized `entity`, and `ChangedProperties` populated with a summary of modified fields. May throw `ArgumentNullException` if `entity` or `originalEntity` is `null`.

## Usage
