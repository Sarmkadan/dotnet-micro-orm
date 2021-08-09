#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using DotnetMicroOrm.Constants;

/// <summary>
/// Extension methods for <see cref="UnitOfWork"/> to provide common transaction patterns
/// and change tracking capabilities.
/// </summary>
public static class UnitOfWorkExtensions
{
    /// <summary>
    /// Executes an operation within a transaction scope
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="operation">The operation to execute within the transaction</param>
    /// <param name="isolationLevel">Optional transaction isolation level</param>
    /// <returns>True if operation succeeded, false otherwise</returns>
    public static async Task<bool> ExecuteInTransactionAsync(
        this UnitOfWork unitOfWork,
        Func<UnitOfWork, Task<bool>> operation,
        TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(operation);

        await unitOfWork.BeginTransactionAsync(isolationLevel);

        try
        {
            var result = await operation(unitOfWork);
            await unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Executes multiple operations within a single transaction
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="operations">Collection of operations to execute within the transaction</param>
    /// <param name="isolationLevel">Optional transaction isolation level</param>
    /// <returns>True if all operations succeeded, false otherwise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unitOfWork"/> or <paramref name="operations"/> is null.</exception>
    public static async Task<bool> ExecuteInTransactionAsync(
        this UnitOfWork unitOfWork,
        IEnumerable<Func<UnitOfWork, Task<bool>>> operations,
        TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        ArgumentNullException.ThrowIfNull(operations);

        await unitOfWork.BeginTransactionAsync(isolationLevel);

        try
        {
            var allSucceeded = true;
            foreach (var operation in operations)
            {
                var result = await operation(unitOfWork);
                if (!result)
                {
                    allSucceeded = false;
                    break;
                }
            }

            if (allSucceeded)
            {
                await unitOfWork.CommitAsync();
                return true;
            }

            await unitOfWork.RollbackAsync();
            return false;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Executes an operation with automatic rollback on failure and change tracking
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="operation">The operation to execute</param>
    /// <returns>Tuple containing operation result and change count</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unitOfWork"/> or <paramref name="operation"/> is null.</exception>
    public static async Task<(bool Success, int Changes)> ExecuteWithTrackingAsync(
        this UnitOfWork unitOfWork,
        Func<UnitOfWork, Task<bool>> operation)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(operation);

        var originalHasChanges = unitOfWork.HasChanges();
        var result = await operation(unitOfWork);
        var changesMade = unitOfWork.HasChanges() && !originalHasChanges;

        return (result, changesMade ? 1 : 0);
    }
}