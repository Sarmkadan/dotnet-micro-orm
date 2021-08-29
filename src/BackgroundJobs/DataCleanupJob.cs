#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;

namespace DotnetMicroOrm.BackgroundJobs;

/// <summary>
/// Background job that removes old audit logs, expired sessions, and soft-deleted records.
/// Runs on a schedule (typically nightly) to maintain database health and performance.
/// Configurable retention periods for different data types.
/// </summary>
public sealed class DataCleanupJob : IBackgroundJob
{
    private readonly IDatabaseContext _dbContext;
    private readonly DataCleanupConfig _config;

    public string JobId => "data-cleanup";
    public string Name => "Data Cleanup Job";
    public string Description => "Removes old audit logs, expired sessions, and archived records";

    private static string Schema => Constants.OrmConstants.DefaultSchema;

    public DataCleanupJob(IDatabaseContext dbContext, DataCleanupConfig? config = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _config = config ?? new DataCleanupConfig();
    }

    public bool CanExecute()
    {
        // Only execute during off-peak hours or if explicitly forced
        var currentHour = DateTime.UtcNow.Hour;
        return currentHour >= 2 && currentHour < 4; // 2-4 AM UTC
    }

    /// <summary>
    /// Runs every enabled cleanup step. Failures propagate so the scheduler can retry
    /// and record the failure.
    /// </summary>
    public async Task ExecuteAsync()
    {
        var tasksRun = 0;
        var rowsAffected = 0;

        if (_config.CleanupAuditLogs)
        {
            rowsAffected += await CleanupAuditLogsAsync();
            tasksRun++;
        }

        if (_config.CleanupSoftDeletedRecords)
        {
            rowsAffected += await CleanupSoftDeletedRecordsAsync();
            tasksRun++;
        }

        if (_config.CleanupTemporaryData)
        {
            rowsAffected += await CleanupTemporaryDataAsync();
            tasksRun++;
        }

        if (_config.RebuildIndexes)
        {
            await RebuildIndexesAsync();
            tasksRun++;
        }

        Console.WriteLine($"Data cleanup job completed. Tasks run: {tasksRun}, rows affected: {rowsAffected}");
    }

    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null</exception>
    public async Task OnFailureAsync(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        Console.Error.WriteLine($"Data cleanup job failed: {ex}");
        await Task.CompletedTask;
    }

    private async Task<int> CleanupAuditLogsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_config.AuditLogRetentionDays);

        var deleted = await DeleteBatchedAsync(
            $"DELETE TOP (@batchSize) FROM [{Schema}].[AuditLogs] WHERE [Timestamp] < @cutoff",
            cutoffDate);

        Console.WriteLine($"Removed {deleted} audit log(s) older than {cutoffDate:u}");
        return deleted;
    }

    private async Task<int> CleanupSoftDeletedRecordsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_config.DeletedRecordRetentionDays);

        var deleted = await DeleteBatchedAsync(
            $"DELETE TOP (@batchSize) FROM [{Schema}].[Products] WHERE [IsActive] = 0 AND [ModifiedDate] < @cutoff",
            cutoffDate);

        Console.WriteLine($"Removed {deleted} soft-deleted record(s) older than {cutoffDate:u}");
        return deleted;
    }

    private async Task<int> CleanupTemporaryDataAsync()
    {
        var cutoffDate = DateTime.UtcNow;

        var deleted = await DeleteBatchedAsync(
            $"DELETE TOP (@batchSize) FROM [{Schema}].[TemporaryData] WHERE [ExpiresAt] < @cutoff",
            cutoffDate);

        Console.WriteLine($"Removed {deleted} expired temporary record(s)");
        return deleted;
    }

    private async Task<int> RebuildIndexesAsync()
    {
        var affected = await _dbContext.ExecuteNonQueryAsync(
            $"ALTER INDEX ALL ON [{Schema}].[AuditLogs] REBUILD");

        Console.WriteLine("Rebuilt indexes on the audit log table");
        return affected;
    }

    /// <summary>
    /// Deletes rows in batches of <see cref="DataCleanupConfig.BatchSize"/> until the
    /// statement stops affecting rows, keeping transaction log growth bounded.
    /// </summary>
    private async Task<int> DeleteBatchedAsync(string statement, DateTime cutoff)
    {
        var total = 0;

        while (true)
        {
            var parameters = new Dictionary<string, object>
            {
                ["batchSize"] = _config.BatchSize,
                ["cutoff"] = cutoff
            };

            var affected = await _dbContext.ExecuteNonQueryAsync(statement, parameters);

            if (affected <= 0)
                break;

            total += affected;

            if (affected < _config.BatchSize)
                break;
        }

        return total;
    }
}

/// <summary>
/// Configuration for data cleanup behavior
/// </summary>
public sealed class DataCleanupConfig
{
    /// <summary>Number of days to retain audit logs</summary>
    public int AuditLogRetentionDays { get; set; } = 90;

    /// <summary>Number of days to retain soft-deleted records</summary>
    public int DeletedRecordRetentionDays { get; set; } = 30;

    /// <summary>Enable audit log cleanup</summary>
    public bool CleanupAuditLogs { get; set; } = true;

    /// <summary>Enable soft-deleted record cleanup</summary>
    public bool CleanupSoftDeletedRecords { get; set; } = true;

    /// <summary>Enable temporary data cleanup</summary>
    public bool CleanupTemporaryData { get; set; } = true;

    /// <summary>Enable database index rebuilding</summary>
    public bool RebuildIndexes { get; set; } = false; // Disable by default as it's heavy

    /// <summary>Batch size for cleanup operations</summary>
    public int BatchSize { get; set; } = 1000;
}
