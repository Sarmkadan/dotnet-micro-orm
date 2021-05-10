#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Collections.Concurrent;
using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// Unit of work pattern implementation for transaction management
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDatabaseContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = [];
    private readonly List<BaseEntity> _changeSet = [];
    private bool _disposed;
    private bool _transactionActive;

    public UnitOfWork(IDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Gets or creates repository for entity type
    public IRepository<T> Repository<T>() where T : BaseEntity, new()
    {
        var type = typeof(T);
        var key = $"{type.FullName}_Repository";

        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new Repository<T>(_context);
            _repositories.TryAdd(type, repository);
        }

        return (IRepository<T>)repository;
    }

    // Begins transaction
    public async Task<bool> BeginTransactionAsync(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.ReadCommitted)
    {
        try
        {
            var result = await _context.BeginTransactionAsync(isolationLevel);
            _transactionActive = result;
            return result;
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("Failed to begin transaction.", ex);
        }
    }

    // Commits transaction
    public async Task<bool> CommitAsync()
    {
        if (!_transactionActive)
            throw new OrmException("No active transaction to commit", "UOW_NO_TRANSACTION");

        try
        {
            var result = await _context.CommitAsync();
            _transactionActive = false;
            _changeSet.Clear();
            return result;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    // Rollbacks transaction
    public async Task<bool> RollbackAsync()
    {
        if (!_transactionActive)
            return true;

        try
        {
            var result = await _context.RollbackAsync();
            _transactionActive = false;
            _changeSet.Clear();
            return result;
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("Transaction rollback failed.", ex);
        }
    }

    // Saves changes
    public async Task<int> SaveChangesAsync()
    {
        if (_changeSet.Count == 0)
            return 0;

        var changeCount = _changeSet.Count;
        _changeSet.Clear();
        return changeCount;
    }

    // Checks if there are pending changes
    public bool HasChanges() => _changeSet.Count > 0;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_transactionActive)
            await RollbackAsync();

        await _context.DisposeAsync();
        _repositories.Clear();
        _changeSet.Clear();
        _disposed = true;
    }
}
