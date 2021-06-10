# DataCleanupJob

`DataCleanupJob` is a configurable background maintenance task responsible for purging stale data and performing index maintenance within the `dotnet-micro-orm` data layer. It consolidates multiple cleanup concerns—audit log trimming, soft-deleted record removal, temporary data expiration, and index rebuilding—into a single job that can be scheduled or triggered manually. Each cleanup domain is independently toggled via boolean flags, and retention windows are controlled through dedicated day-count properties.

## API

### `public DataCleanupJob`

The parameterless constructor. Instantiates the job with default retention values and all cleanup flags set to `false`. Callers must explicitly configure the desired cleanup targets and retention periods before invoking `ExecuteAsync`.

### `public bool CanExecute`

Returns `true` if at least one cleanup operation is enabled (`CleanupAuditLogs`, `CleanupSoftDeletedRecords`, `CleanupTemporaryData`, or `RebuildIndexes`) and the associated retention or batch parameters are valid. Returns `false` otherwise. This property should be checked before calling `ExecuteAsync` to avoid a no-op execution.

### `public async Task ExecuteAsync`

Executes all enabled cleanup operations sequentially. Each operation runs in its own transaction where applicable. The method respects `BatchSize` for bulk deletions.  
**Throws** `InvalidOperationException` when `CanExecute` is `false` at the moment execution begins.  
**Throws** database-layer exceptions (e.g., timeout, connection failure) wrapped in the ORM’s standard exception types if the underlying commands fail.

### `public async Task OnFailureAsync`

Invoked automatically by the job infrastructure when `ExecuteAsync` throws. The default implementation logs the failure details and preserves the original exception. Override this method in derived classes to add custom alerting or compensating actions.  
**Parameters:** none.  
**Returns:** a completed task after logging.

### `public int AuditLogRetentionDays`

Number of days of audit log history to preserve. Records older than this threshold are deleted when `CleanupAuditLogs` is `true`. Must be greater than zero; setting a value ≤ 0 causes `CanExecute` to return `false`.

### `public int DeletedRecordRetentionDays`

Number of days to retain soft-deleted records before permanent removal. Applies when `CleanupSoftDeletedRecords` is `true`. Must be greater than zero; otherwise `CanExecute` returns `false`.

### `public bool CleanupAuditLogs`

When `true`, `ExecuteAsync` purges audit log entries exceeding `AuditLogRetentionDays`.

### `public bool CleanupSoftDeletedRecords`

When `true`, `ExecuteAsync` permanently deletes records that have been soft-deleted for longer than `DeletedRecordRetentionDays`.

### `public bool CleanupTemporaryData`

When `true`, `ExecuteAsync` removes rows marked as temporary (e.g., session data, staging records) that have exceeded their built-in expiry. This operation does not use a configurable retention property; it relies on an internal expiry timestamp column.

### `public bool RebuildIndexes`

When `true`, `ExecuteAsync` performs an index rebuild or reorganize operation on target tables. This is typically a heavy operation and ignores `BatchSize`.

### `public int BatchSize`

Maximum number of rows to delete per batch statement during cleanup operations. Applies to `CleanupAuditLogs`, `CleanupSoftDeletedRecords`, and `CleanupTemporaryData`. Must be greater than zero when any of those flags are enabled; otherwise `CanExecute` returns `false`.

## Usage

**Example 1: Scheduled nightly cleanup with audit and soft-delete purging**

```csharp
var job = new DataCleanupJob
{
    CleanupAuditLogs = true,
    AuditLogRetentionDays = 90,
    CleanupSoftDeletedRecords = true,
    DeletedRecordRetentionDays = 30,
    BatchSize = 1000
};

if (job.CanExecute)
{
    try
    {
        await job.ExecuteAsync();
    }
    catch (Exception ex)
    {
        await job.OnFailureAsync();
        // Additional alerting logic here
    }
}
```

**Example 2: Weekend maintenance window including index rebuild**

```csharp
var job = new DataCleanupJob
{
    CleanupTemporaryData = true,
    RebuildIndexes = true,
    BatchSize = 500
};

if (job.CanExecute)
{
    await job.ExecuteAsync();
}
```

## Notes

- **Order of execution:** Operations run sequentially in the order: audit logs, soft-deleted records, temporary data, index rebuild. A failure in an early step prevents later steps from executing.
- **Thread safety:** `DataCleanupJob` is not designed for concurrent use. Do not share instances across threads or invoke `ExecuteAsync` on the same instance while a prior execution is still in flight.
- **`CanExecute` volatility:** Changing configuration properties after calling `CanExecute` but before `ExecuteAsync` can lead to an `InvalidOperationException`. Re-check `CanExecute` immediately before execution if configuration is mutated dynamically.
- **`BatchSize` and large backlogs:** When retention windows are large or the job has never run, the first execution may process millions of rows. Set `BatchSize` conservatively to avoid long-running transactions and excessive locking.
- **`RebuildIndexes` impact:** This operation can block writes and consume significant I/O. It ignores `BatchSize` entirely. Schedule it during low-traffic periods and consider disabling it when other cleanup operations are sufficient.
- **`OnFailureAsync` override:** If overridden in a derived class, always call `base.OnFailureAsync()` to preserve the default logging behavior unless you intentionally want to suppress it.
- **Retention day validation:** Setting `AuditLogRetentionDays` or `DeletedRecordRetentionDays` to zero or negative values causes `CanExecute` to return `false` even if the corresponding boolean flag is `true`.
