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

    public async Task ExecuteAsync()
    {
        var tasksRun = 0;

        if (_config.CleanupAuditLogs)
        {
            await CleanupAuditLogsAsync();
            tasksRun++;
        }

        if (_config.CleanupSoftDeletedRecords)
        {
            await CleanupSoftDeletedRecordsAsync();
            tasksRun++;
        }

        if (_config.CleanupTemporaryData)
        {
            await CleanupTemporaryDataAsync();
            tasksRun++;
        }

        if (_config.RebuildIndexes)
        {
            await RebuildIndexesAsync();
            tasksRun++;
        }

        Console.WriteLine($"Data cleanup job completed. Tasks run: {tasksRun}");
    }

    public async Task OnFailureAsync(Exception ex)
    {
        Console.WriteLine($"Data cleanup job failed: {ex.Message}");
        // In production, would log to monitoring system or alert admins
        await Task.CompletedTask;
    }

    private async Task CleanupAuditLogsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_config.AuditLogRetentionDays);

        try
        {
            // This would use the actual repository to delete old audit logs
            Console.WriteLine($"Removing audit logs older than {cutoffDate:G}");
            await Task.Delay(100); // Simulate work
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to cleanup audit logs: {ex.Message}");
        }
    }

    private async Task CleanupSoftDeletedRecordsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_config.DeletedRecordRetentionDays);

        try
        {
            // Soft-deleted records older than retention period are permanently deleted
            Console.WriteLine($"Removing soft-deleted records older than {cutoffDate:G}");
            await Task.Delay(100); // Simulate work
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to cleanup soft-deleted records: {ex.Message}");
        }
    }

    private async Task CleanupTemporaryDataAsync()
    {
        try
        {
            // Remove expired temporary data, sessions, tokens, etc
            Console.WriteLine("Cleaning up temporary and expired data");
            await Task.Delay(100); // Simulate work
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to cleanup temporary data: {ex.Message}");
        }
    }

    private async Task RebuildIndexesAsync()
    {
        try
        {
            // Rebuild fragmented indexes for performance
            Console.WriteLine("Rebuilding database indexes");
            await Task.Delay(100); // Simulate work
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to rebuild indexes: {ex.Message}");
        }
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
