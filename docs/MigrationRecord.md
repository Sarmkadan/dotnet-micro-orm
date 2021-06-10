# MigrationRecord

Represents a record of a database migration that has been applied to the schema. It tracks the migration version, description, execution outcome, and any errors that occurred during application.

## API

### `public int Id`
A unique identifier for the migration record. Typically auto-incremented by the database.

### `public string Version`
The version identifier of the migration (e.g., "20240515120000"). Used to correlate the record with the migration file or script.

### `public string Description`
A human-readable description of the migration's purpose or changes.

### `public DateTime AppliedAt`
The timestamp when the migration was successfully applied to the database.

### `public bool Success`
Indicates whether the migration was applied successfully (`true`) or encountered an error (`false`).

### `public string? ErrorMessage`
Contains the error message if the migration failed (`Success` is `false`); otherwise, `null`.

## Usage
