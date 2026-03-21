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

    public AuditService(IDatabaseContext context)
    {
        _context = context;
        _auditRepository = new Repository<AuditLog>(context);
    }

    // Logs entity insert
    public async Task<AuditLog> LogInsertAsync(string entityType, int entityId, string? newValues = null, int? userId = null, string? username = null)
    {
        var log = AuditLog.CreateInsert(entityType, entityId, newValues, userId, username);
        log.MarkAsSuccess();
        return await _auditRepository.AddAsync(log);
    }

    // Logs entity update
    public async Task<AuditLog> LogUpdateAsync(string entityType, int entityId, string? oldValues = null, string? newValues = null, string? changedProperties = null, int? userId = null, string? username = null)
    {
        var log = AuditLog.CreateUpdate(entityType, entityId, oldValues, newValues, changedProperties, userId, username);
        log.MarkAsSuccess();
        return await _auditRepository.AddAsync(log);
    }

    // Logs entity delete
    public async Task<AuditLog> LogDeleteAsync(string entityType, int entityId, string? oldValues = null, int? userId = null, string? username = null)
    {
        var log = AuditLog.CreateDelete(entityType, entityId, oldValues, userId, username);
        log.MarkAsSuccess();
        return await _auditRepository.AddAsync(log);
    }

    // Logs operation failure
    public async Task<AuditLog> LogFailureAsync(string entityType, int entityId, string action, string errorMessage, int? userId = null, string? username = null)
    {
        var log = new AuditLog(entityType, entityId, action)
        {
            UserId = userId,
            Username = username
        };

        log.MarkAsFailure(errorMessage);
        return await _auditRepository.AddAsync(log);
    }

    // Gets audit logs for entity
    public async Task<List<AuditLog>> GetAuditLogsAsync(string entityType, int entityId)
    {
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.EntityType == entityType && l.EntityId == entityId)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    // Gets user activity logs
    public async Task<List<AuditLog>> GetUserActivityAsync(int userId)
    {
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.UserId == userId)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    // Gets logs by action type
    public async Task<List<AuditLog>> GetLogsByActionAsync(string action)
    {
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.Action.Equals(action, StringComparison.OrdinalIgnoreCase))
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    // Gets failed operations
    public async Task<List<AuditLog>> GetFailedOperationsAsync(int daysBack = 7)
    {
        var logs = await _auditRepository.GetAllAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);

        return logs.Where(l => !l.IsSuccessful && l.Timestamp >= cutoffDate)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    // Gets logs by date range
    public async Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var logs = await _auditRepository.GetAllAsync();
        return logs.Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                   .OrderByDescending(l => l.Timestamp)
                   .ToList();
    }

    // Purges old logs
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

    // Gets summary statistics
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
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public int Inserts { get; set; }
    public int Updates { get; set; }
    public int Deletes { get; set; }

    public decimal SuccessRate => TotalOperations > 0 ? (decimal)SuccessfulOperations / TotalOperations * 100 : 0;
}
