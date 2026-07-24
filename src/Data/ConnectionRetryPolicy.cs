#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

/// <summary>
/// Configuration for retrying transient database failures with exponential backoff and jitter.
/// Attach an instance to <see cref="DatabaseContext"/> to opt connection opening and command
/// execution into automatic retries.
/// </summary>
public sealed class ConnectionRetryPolicy
{
    private readonly Random _random = new();

    /// <summary>
    /// Maximum number of attempts, including the first, before the failure propagates to the caller.
    /// Defaults to <c>3</c>. Must be at least <c>1</c>.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay used to compute the exponential backoff for the first retry. Defaults to <c>200ms</c>.
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Upper bound applied to the computed backoff delay, after jitter. Defaults to <c>5</c> seconds.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Fraction (0.0-1.0) of the computed delay randomly added or subtracted to avoid synchronized
    /// retry storms across concurrent callers. Defaults to <c>0.2</c> (±20%).
    /// </summary>
    public double JitterFactor { get; set; } = 0.2;

    /// <summary>
    /// Determines whether a given exception, raised while targeting the given provider, should be
    /// retried. Defaults to <see cref="DefaultTransientClassifier"/>; assign a custom delegate to
    /// recognize provider-specific transient conditions beyond the built-in defaults.
    /// </summary>
    public Func<Exception, DatabaseProvider, bool> TransientErrorClassifier { get; set; } = DefaultTransientClassifier;

    /// <summary>
    /// A no-op policy that performs exactly one attempt and never retries. Useful as an explicit
    /// opt-out default for callers that construct a <see cref="DatabaseContext"/> without retry.
    /// </summary>
    public static ConnectionRetryPolicy None => new() { MaxAttempts = 1 };

    /// <summary>
    /// Executes <paramref name="operation"/>, retrying with exponential backoff and jitter whenever
    /// the raised exception is classified as transient for <paramref name="provider"/> and attempts
    /// remain. The final, non-retried exception (transient or not) propagates unchanged to the caller.
    /// </summary>
    /// <typeparam name="T">Result type produced by <paramref name="operation"/>.</typeparam>
    /// <param name="operation">The asynchronous operation to invoke and potentially retry.</param>
    /// <param name="provider">The database provider being targeted, used for error classification.</param>
    /// <param name="cancellationToken">Token observed between attempts and before each retry delay.</param>
    /// <returns>The result produced by <paramref name="operation"/> once it succeeds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operation"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, DatabaseProvider provider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var attempt = 0;
        while (true)
        {
            attempt++;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < MaxAttempts && IsRetryable(ex, provider))
            {
                await Task.Delay(ComputeDelay(attempt), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private bool IsRetryable(Exception exception, DatabaseProvider provider) => exception switch
    {
        DatabaseConnectionException dbEx => dbEx.IsTransient,
        _ => TransientErrorClassifier(exception, provider)
    };

    private TimeSpan ComputeDelay(int attempt)
    {
        var exponentialMs = BaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        var cappedMs = Math.Min(exponentialMs, MaxDelay.TotalMilliseconds);
        var jitterRangeMs = cappedMs * JitterFactor;
        var jitteredMs = cappedMs + (_random.NextDouble() * 2 - 1) * jitterRangeMs;
        var clampedMs = Math.Clamp(jitteredMs, 0, MaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(clampedMs);
    }

    /// <summary>
    /// Default transient-error classifier. Recognizes common SQL Server transient error numbers
    /// (connection timeouts, deadlock victims, resource throttling, transient network failures) and
    /// SQLite busy/locked result codes. Unrecognized exceptions and providers are treated as non-transient.
    /// </summary>
    /// <param name="exception">The exception raised by the failed operation.</param>
    /// <param name="provider">The database provider the operation targeted.</param>
    /// <returns><c>true</c> when the failure is considered transient and worth retrying.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool DefaultTransientClassifier(Exception exception, DatabaseProvider provider)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            SqlException sqlEx when provider == DatabaseProvider.SqlServer => IsTransientSqlServerErrorNumber(sqlEx.Number),
            SqliteException sqliteEx when provider == DatabaseProvider.Sqlite => IsTransientSqliteErrorCode(sqliteEx.SqliteErrorCode),
            TimeoutException => true,
            _ => false
        };
    }

    private static bool IsTransientSqlServerErrorNumber(int errorNumber) => errorNumber switch
    {
        -2 => true,     // client-side command/connection timeout
        1205 => true,   // deadlock victim
        4060 => true,   // cannot open database, transient during failover
        10053 => true,  // transport-level error, connection aborted
        10054 => true,  // transport-level error, connection reset
        10060 => true,  // network timeout establishing connection
        10928 => true,  // resource limit reached (Azure SQL)
        10929 => true,  // resource limit reached (Azure SQL)
        40197 => true,  // service busy processing request (Azure SQL)
        40501 => true,  // service currently busy (Azure SQL)
        40613 => true,  // database unavailable, failover in progress (Azure SQL)
        49918 => true,  // cannot process request, not enough resources (Azure SQL)
        49919 => true,  // cannot process create/update request, too many operations (Azure SQL)
        49920 => true,  // cannot process request, too many operations in progress (Azure SQL)
        _ => false
    };

    private static bool IsTransientSqliteErrorCode(int sqliteErrorCode) => sqliteErrorCode switch
    {
        5 => true, // SQLITE_BUSY - database file locked by another connection
        6 => true, // SQLITE_LOCKED - table locked within the same connection
        _ => false
    };
}
