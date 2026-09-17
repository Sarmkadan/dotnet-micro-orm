#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Services;

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Audit service contract for tracking entity changes and system operations.
/// </summary>
public interface IAuditService
{
    Task<AuditLog> LogInsertAsync(string entityType, int entityId, string? newValues = null, int? userId = null, string? username = null);
    Task<AuditLog> LogUpdateAsync(string entityType, int entityId, string? oldValues = null, string? newValues = null, string? changedProperties = null, int? userId = null, string? username = null);
    Task<AuditLog> LogDeleteAsync(string entityType, int entityId, string? oldValues = null, int? userId = null, string? username = null);
    Task<AuditLog> LogFailureAsync(string entityType, int entityId, string action, string errorMessage, int? userId = null, string? username = null);
    Task<List<AuditLog>> GetAuditLogsAsync(string entityType, int entityId);
    Task<List<AuditLog>> GetUserActivityAsync(int userId);
    Task<List<AuditLog>> GetLogsByActionAsync(string action);
    Task<List<AuditLog>> GetFailedOperationsAsync(int daysBack = 7);
    Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> PurgeOldLogsAsync(int daysToKeep = 90);
    Task<AuditSummary> GetSummaryAsync();
}

/// <summary>
/// Audit service for tracking entity changes and system operations
/// </summary>
public sealed class AuditService : IAuditService, IAsyncDisposable
{
    private readonly IRepository<AuditLog> _auditRepository;
    private readonly IDatabaseContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="context">The database context used to access the audit log repository.</param>
    public AuditService(IDatabaseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _auditRepository = new Repository<AuditLog>(context);
    }

    /// <summary>
    /// Logs an entity insert operation.
    /// </summary>
    /// <param name="entityType">The type of the entity being inserted.</param>
    /// <param name="entityId">The identifier of the entity being inserted.</param>
    /// <param name="newValues">The serialized values of the entity after the insert.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="username">The name of the user performing the operation.</param>
    /// <returns>The created audit log entry.</returns>
    public async Task<AuditLog> LogInsertAsync(string entityType, int entityId, string? newValues = null, int? userId = null, string? username = null)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        var log = AuditLog.CreateInsert(entityType, entityId, newValues, userId, username);
        log.MarkAsSuccess();
        return await _auditRepository.AddAsync(log);
    }

    /// <summary>
    /// Logs an entity update operation.
    /// </summary>
    /// <param name="entityType">The type of the entity being updated.</param>
    /// <param name="entityId">The identifier of the entity being updated.</param>
    /// <param name="oldValues">The serialized values of the entity before the update.</param>
    /// <param name="newValues">The serialized values of the entity after the update.</param>
    /// <param name="changedProperties">The names of the properties that changed.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="username">The name of the user performing the operation.</param>
    /// <returns>The created audit log entry.</returns>
    public async Task<AuditLog> LogUpdateAsync(string entityType, int entityId, string? oldValues = null, string? newValues = null, string? changedProperties = null, int? userId = null, string? username = null)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        var log = AuditLog.CreateUpdate(entityType, entityId, oldValues, newValues, changedProperties, userId, username);
        log.MarkAsSuccess();
        return await _auditRepository.AddAsync(log);
    }

    /// <summary>
    /// Logs an entity delete operation.
    /// </summary>
    /// <param name="entityType">The type of the entity being deleted.</param>
    /// <param name="entityId">The identifier of the entity being deleted.</param>
    /// <param name="oldValues">The serialized values of the entity before the delete.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="username">The name of the user performing the operation.</param>
    /// <returns>The created audit log entry.</returns>
    public async Task<AuditLog> LogDeleteAsync(string entityType, int entityId, string? oldValues = null, int? userId = null, string? username = null)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        var log = AuditLog.CreateDelete(entityType, entityId, oldValues, userId, username);
        log.MarkAsSuccess();
        return await _auditRepository.AddAsync(log);
    }

    /// <summary>
    /// Logs a failed operation.
    /// </summary>
    /// <param name="entityType">The type of the entity involved in the failed operation.</param>
    /// <param name="entityId">The identifier of the entity involved in the failed operation.</param>
    /// <param name="action">The action that failed.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="username">The name of the user performing the operation.</param>
    /// <returns>The created audit log entry.</returns>
    public async Task<AuditLog> LogFailureAsync(string entityType, int entityId, string action, string errorMessage, int? userId = null, string? username = null)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(errorMessage);
        var log = new AuditLog(entityType, entityId, action)
        {
            UserId = userId,
            Username = username
        };

        log.MarkAsFailure(errorMessage);
        return await _auditRepository.AddAsync(log);
    }

    /// <summary>
    /// Gets the audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="entityId">The identifier of the entity.</param>
    /// <returns>The audit log entries for the entity, ordered by timestamp descending.</returns>
    public async Task<List<AuditLog>> GetAuditLogsAsync(string entityType, int entityId)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.EntityType == entityType && l.EntityId == entityId)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    /// <summary>
    /// Gets the audit logs for a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The audit log entries for the user, ordered by timestamp descending.</returns>
    public async Task<List<AuditLog>> GetUserActivityAsync(int userId)
    {
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.UserId == userId)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    /// <summary>
    /// Gets the audit logs for a specific action type.
    /// </summary>
    /// <param name="action">The action type to filter by.</param>
    /// <returns>The audit log entries matching the action, ordered by timestamp descending.</returns>
    public async Task<List<AuditLog>> GetLogsByActionAsync(string action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.Action.Equals(action, StringComparison.OrdinalIgnoreCase))
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    /// <summary>
    /// Gets the failed operations within the specified number of days.
    /// </summary>
    /// <param name="daysBack">The number of days to look back from the current time.</param>
    /// <returns>The failed audit log entries, ordered by timestamp descending.</returns>
    public async Task<List<AuditLog>> GetFailedOperationsAsync(int daysBack = 7)
    {
        var logs = await _auditRepository.GetAllAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

        return logs.Where(l => !l.IsSuccessful && l.Timestamp >= cutoffDate)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    /// <summary>
    /// Gets the audit logs within the specified date range.
    /// </summary>
    /// <param name="startDate">The inclusive start of the date range.</param>
    /// <param name="endDate">The inclusive end of the date range.</param>
    /// <returns>The audit log entries within the date range, ordered by timestamp descending.</returns>
    public async Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    /// <summary>
    /// Purges audit logs older than the specified number of days.
    /// </summary>
    /// <param name="daysToKeep">The number of days of logs to retain.</param>
    /// <returns>The number of audit log entries deleted.</returns>
    public async Task<int> PurgeOldLogsAsync(int daysToKeep = 90)
    {
        var logs = await _auditRepository.GetAllAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var logsToDelete = logs.Where(l => l.Timestamp < cutoffDate).ToList();

        var deletedCount = 0;
        foreach (var log in logsToDelete)
        {
            if (await _auditRepository.DeleteAsync(log))
                deletedCount++;
        }

        return deletedCount;
    }

    /// <summary>
    /// Gets summary statistics for all audit logs.
    /// </summary>
    /// <returns>The audit summary statistics.</returns>
    public async Task<AuditSummary> GetSummaryAsync()
    {
        var logs = await _auditRepository.GetAllAsync();
        return new AuditSummary
        {
            TotalOperations = logs.Count,
            SuccessfulOperations = logs.Count(l => l.IsSuccessful),
            FailedOperations = logs.Count(l => !l.IsSuccessful),
            Inserts = logs.Count(l => l.Action == "INSERT"),
            Updates = logs.Count(l => l.Action == "UPDATE"),
            Deletes = logs.Count(l => l.Action == "DELETE")
        };
    }

    /// <summary>
    /// Releases the database context resources asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}

/// <summary>
/// Audit summary statistics
/// </summary>
public sealed class AuditSummary
{
    /// <summary>Gets or sets the total number of operations.</summary>
    public int TotalOperations { get; set; }
    /// <summary>Gets or sets the number of successful operations.</summary>
    public int SuccessfulOperations { get; set; }
    /// <summary>Gets or sets the number of failed operations.</summary>
    public int FailedOperations { get; set; }
    /// <summary>Gets or sets the number of insert operations.</summary>
    public int Inserts { get; set; }
    /// <summary>Gets or sets the number of update operations.</summary>
    public int Updates { get; set; }
    /// <summary>Gets or sets the number of delete operations.</summary>
    public int Deletes { get; set; }

    /// <summary>Gets the percentage of operations that succeeded.</summary>
    public decimal SuccessRate => TotalOperations > 0 ? (decimal)SuccessfulOperations / TotalOperations * 100 : 0;
}
