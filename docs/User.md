# User

Represents a user entity in the system with core authentication and profile information. Tracks account status, login history, and maintains a collection of associated orders.

## API

### Properties

#### `public int Id`
Unique identifier for the user. Assigned by the persistence layer and immutable after creation.

#### `public string Username`
User's login identifier. Must be unique across the system and is validated during account creation.

#### `public string Email`
User's email address. Serves as the primary contact method and must be unique. Used for authentication and notifications.

#### `public string PasswordHash`
Securely hashed representation of the user's password. Never stored in plain text; validated against during authentication.

#### `public string? FirstName`
User's given name. Optional field that may be null for privacy or incomplete registration scenarios.

#### `public string? LastName`
User's family name. Optional field that may be null for privacy or incomplete registration scenarios.

#### `public string? PhoneNumber`
User's contact number. Optional field that may be null for privacy or incomplete registration scenarios.

#### `public bool IsActive`
Indicates whether the user account is currently active and usable. Deactivated accounts cannot log in.

#### `public bool IsEmailVerified`
Indicates whether the user's email address has been verified via confirmation link. Required for certain sensitive operations.

#### `public DateTime? LastLoginDate`
Timestamp of the user's most recent successful login. Null if the user has never logged in.

#### `public DateTime CreatedDate`
Timestamp when the user account was created. Set automatically and immutable after creation.

#### `public DateTime? ModifiedDate`
Timestamp when the user account was last modified. Null if the account has never been updated.

#### `public int Version`
Optimistic concurrency control version number. Incremented on each modification to prevent concurrent update conflicts.

#### `public virtual List<Order> Orders`
Collection of orders associated with this user. Lazy-loaded by default via ORM configuration.

### Constructors

#### `public User()`
Initializes a new user instance with default values. All string properties are initialized to empty strings, nullable strings to null, booleans to false, and date properties to null.

### Methods

#### `public override bool Validate()`
Validates the current user instance. Returns `true` if all required fields are valid; otherwise, returns `false`. Required fields include `Username`, `Email`, and `PasswordHash`. Throws no exceptions.

#### `public string GetFullName()`
Returns the user's full name by concatenating `FirstName` and `LastName` with a space separator. If either name is null, it is omitted from the result. Returns an empty string if both names are null.

#### `public void MarkAsEmailVerified()`
Marks the user's email as verified by setting `IsEmailVerified` to `true` and updating `ModifiedDate` to the current UTC timestamp.

#### `public void UpdateLastLogin()`
Updates the user's last login timestamp to the current UTC time and sets `ModifiedDate` to the same value. Does not modify any other properties.

#### `public void Deactivate()`
Deactivates the user account by setting `IsActive` to `false` and updating `ModifiedDate` to the current UTC timestamp.

## Usage
