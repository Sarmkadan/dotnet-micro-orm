#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Migrations;

using System.Globalization;
using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// Runs registered <see cref="IMigration"/> implementations in version order,
/// persisting state to a <c>_MigrationHistory</c> table that is created
/// automatically on first use.
/// </summary>
public sealed class MigrationRunner : IMigrationRunner
{
    private readonly IDatabaseContext _context;
    private readonly IReadOnlyList<IMigration> _migrations;

    private const string HistoryTable = "_MigrationHistory";
    private const string Schema = "dbo";

    internal IDatabaseContext Context => _context;
    internal IReadOnlyList<IMigration> Migrations => _migrations;

    /// <summary>
    /// Creates a new runner.
    /// </summary>
    /// <param name="context">Database context used to apply migrations.</param>
    /// <param name="migrations">
    /// All discovered migrations.  They will be sorted by <see cref="IMigration.Version"/>
    /// regardless of the order they are supplied.
    /// </param>
    public MigrationRunner(IDatabaseContext context, IEnumerable<IMigration> migrations)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _migrations = migrations
            .OrderBy(m => m.Version, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task MigrateAsync()
    {
        await EnsureHistoryTableAsync();
        var applied = (await GetAppliedVersionsAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var migration in _migrations.Where(m => !applied.Contains(m.Version)))
            await ApplyMigrationAsync(migration);
    }

    /// <inheritdoc/>
    public async Task MigrateToAsync(string targetVersion)
    {
        if (string.IsNullOrWhiteSpace(targetVersion))
            throw new ArgumentException("Target version must not be empty.", nameof(targetVersion));

        await EnsureHistoryTableAsync();
        var applied = (await GetAppliedVersionsAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var migration in _migrations
            .Where(m => string.Compare(m.Version, targetVersion, StringComparison.OrdinalIgnoreCase) <= 0
                        && !applied.Contains(m.Version)))
        {
            await ApplyMigrationAsync(migration);
        }
    }

    /// <inheritdoc/>
    public async Task RollbackToAsync(string targetVersion)
    {
        if (string.IsNullOrWhiteSpace(targetVersion))
            throw new ArgumentException("Target version must not be empty.", nameof(targetVersion));

        await EnsureHistoryTableAsync();
        var applied = (await GetAppliedVersionsAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Roll back in descending order from the most-recently applied down to (but not including) targetVersion
        foreach (var migration in _migrations
            .Where(m => string.Compare(m.Version, targetVersion, StringComparison.OrdinalIgnoreCase) > 0
                        && applied.Contains(m.Version))
            .OrderByDescending(m => m.Version, StringComparer.OrdinalIgnoreCase))
        {
            await RevertMigrationAsync(migration);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MigrationRecord>> GetAppliedMigrationsAsync()
    {
        await EnsureHistoryTableAsync();

        var query = $"SELECT [Id], [Version], [Description], [AppliedAt], [Success], [ErrorMessage] " +
                    $"FROM [{Schema}].[{HistoryTable}] ORDER BY [Version]";

        var rows = await _context.ExecuteQueryAsync(query);
        return rows.Select(MapRow).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync()
    {
        await EnsureHistoryTableAsync();
        var applied = (await GetAppliedVersionsAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return _migrations
            .Where(m => !applied.Contains(m.Version))
            .Select(m => m.Version)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IMigrationRunner.PendingMigration>> GetPendingMigrationsWithDetailsAsync()
    {
        await EnsureHistoryTableAsync();
        var applied = (await GetAppliedVersionsAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var pendingMigrations = new List<IMigrationRunner.PendingMigration>();

        foreach (var migration in _migrations.Where(m => !applied.Contains(m.Version))
                     .OrderBy(m => m.Version, StringComparer.OrdinalIgnoreCase))
        {
            pendingMigrations.Add(new IMigrationRunner.PendingMigration
            {
                Version = migration.Version,
                Description = migration.Description,
                UpSql = await GenerateSqlInternalAsync(migration, isUp: true)
            });
        }

        return pendingMigrations;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateUpSqlAsync(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version must not be empty.", nameof(version));

        var migration = _migrations.FirstOrDefault(m => string.Equals(m.Version, version, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"No migration found with version '{version}'.");

        return await GenerateSqlInternalAsync(migration, isUp: true);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateDownSqlAsync(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version must not be empty.", nameof(version));

        var migration = _migrations.FirstOrDefault(m => string.Equals(m.Version, version, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"No migration found with version '{version}'.");

        return await GenerateSqlInternalAsync(migration, isUp: false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> MigrateDryRunAsync(string? targetVersion = null)
    {
        if (!string.IsNullOrWhiteSpace(targetVersion) && string.IsNullOrWhiteSpace(targetVersion.Trim()))
            throw new ArgumentException("Target version must not be whitespace.", nameof(targetVersion));

        await EnsureHistoryTableAsync();
        var applied = (await GetAppliedVersionsAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var migrationsToApply = targetVersion == null
            ? _migrations.Where(m => !applied.Contains(m.Version))
            : _migrations
                .Where(m => string.Compare(m.Version, targetVersion, StringComparison.OrdinalIgnoreCase) <= 0
                            && !applied.Contains(m.Version));

        var scripts = new List<string>();

        foreach (var migration in migrationsToApply.OrderBy(m => m.Version, StringComparer.OrdinalIgnoreCase))
        {
            scripts.Add(await GenerateSqlInternalAsync(migration, isUp: true));
        }

        return scripts;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> RollbackDryRunAsync(string targetVersion)
    {
        if (string.IsNullOrWhiteSpace(targetVersion))
            throw new ArgumentException("Target version must not be empty.", nameof(targetVersion));

        await EnsureHistoryTableAsync();
        var applied = (await GetAppliedVersionsAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var migrationsToRollback = _migrations
            .Where(m => string.Compare(m.Version, targetVersion, StringComparison.OrdinalIgnoreCase) > 0
                        && applied.Contains(m.Version))
            .OrderByDescending(m => m.Version, StringComparer.OrdinalIgnoreCase);

        var scripts = new List<string>();

        foreach (var migration in migrationsToRollback)
        {
            scripts.Add(await GenerateSqlInternalAsync(migration, isUp: false));
        }

        return scripts;
    }

    // Generates SQL for a migration by executing it against a mock context that captures SQL.
    private async Task<string> GenerateSqlInternalAsync(IMigration migration, bool isUp)
    {
        // Create a simple mock context that captures SQL statements
        var mockContext = new SqlCaptureContext();

        if (isUp)
        {
            await migration.UpAsync(mockContext);
        }
        else
        {
            await migration.DownAsync(mockContext);
        }

        return string.Join("\n\n", mockContext.CapturedSql);
    }

    // Simple mock IDatabaseContext that captures SQL statements without executing them
    private sealed class SqlCaptureContext : IDatabaseContext
    {
        public List<string> CapturedSql { get; } = [];

        public Task<bool> OpenAsync() => Task.FromResult(true);
        public Task<bool> CloseAsync() => Task.FromResult(true);
        public Task<bool> TestConnectionAsync() => Task.FromResult(true);
        public Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null)
        {
            CapturedSql.Add(query);
            return Task.FromResult<object?>(null);
        }

        public Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            CapturedSql.Add(query);
            return Task.FromResult<List<Dictionary<string, object>>>([]);
        }

        public IAsyncEnumerable<Dictionary<string, object>> ExecuteStreamAsync(string query, Dictionary<string, object>? parameters = null)
        {
            CapturedSql.Add(query);
            return Array.Empty<Dictionary<string, object>>().ToAsyncEnumerable();
        }

        public Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            CapturedSql.Add(query);
            return Task.FromResult(1);
        }

        public Task<int> ExecuteNonQueryAsync(string query, object? parameters = null)
        {
            CapturedSql.Add(query);
            return Task.FromResult(1);
        }

        public Task<bool> BeginTransactionAsync(TransactionIsolationLevel isolationLevel) => Task.FromResult(true);
        public Task<bool> CommitAsync() => Task.FromResult(true);
        public Task<bool> RollbackAsync() => Task.FromResult(true);
        public DatabaseProvider GetDatabaseProvider() => DatabaseProvider.Sqlite;
        public string GetConnectionString() => "Data Source=:memory:";

        public void Dispose() { }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    // Runs a migration's Up script and records the result in the history table.
    private async Task ApplyMigrationAsync(IMigration migration)
    {
        string? errorMessage = null;
        bool success = true;

        try
        {
            await migration.UpAsync(_context);
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            throw new OrmException(
                $"Migration '{migration.Version}' ({migration.Description}) failed: {ex.Message}",
                "MIGRATION_FAILED", ex);
        }
        finally
        {
            await RecordMigrationAsync(migration, success, errorMessage);
        }
    }

    // Runs a migration's Down script and removes the record from the history table.
    private async Task RevertMigrationAsync(IMigration migration)
    {
        try
        {
            await migration.DownAsync(_context);
        }
        catch (Exception ex)
        {
            throw new OrmException(
                $"Rollback of migration '{migration.Version}' ({migration.Description}) failed: {ex.Message}",
                "MIGRATION_ROLLBACK_FAILED", ex);
        }

        var deleteQuery = $"DELETE FROM [{Schema}].[{HistoryTable}] WHERE [Version] = @Version";
        await _context.ExecuteNonQueryAsync(deleteQuery, new Dictionary<string, object> { ["Version"] = migration.Version });
    }

    // Creates the _MigrationHistory table if it does not already exist.
    private async Task EnsureHistoryTableAsync()
    {
        var ddl = $"""
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = '{Schema}' AND TABLE_NAME = '{HistoryTable}'
            )
            BEGIN
                CREATE TABLE [{Schema}].[{HistoryTable}] (
                    [Id]           INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    [Version]      NVARCHAR(50)  NOT NULL,
                    [Description]  NVARCHAR(500) NOT NULL,
                    [AppliedAt]    DATETIME2     NOT NULL,
                    [Success]      BIT           NOT NULL,
                    [ErrorMessage] NVARCHAR(MAX) NULL,
                    CONSTRAINT [UQ_{HistoryTable}_Version] UNIQUE ([Version])
                )
            END
            """;

        await _context.ExecuteNonQueryAsync(ddl);
    }

    private async Task RecordMigrationAsync(IMigration migration, bool success, string? errorMessage)
    {
        var insertQuery = $"""
            INSERT INTO [{Schema}].[{HistoryTable}]
                ([Version], [Description], [AppliedAt], [Success], [ErrorMessage])
            VALUES
                (@Version, @Description, @AppliedAt, @Success, @ErrorMessage)
            """;

        await _context.ExecuteNonQueryAsync(insertQuery, new Dictionary<string, object>
        {
            ["Version"]      = migration.Version,
            ["Description"]  = migration.Description,
            ["AppliedAt"]    = DateTime.UtcNow,
            ["Success"]      = success,
            ["ErrorMessage"] = (object?)errorMessage ?? DBNull.Value
        });
    }

    private async Task<IEnumerable<string>> GetAppliedVersionsAsync()
    {
        var query = $"SELECT [Version] FROM [{Schema}].[{HistoryTable}] WHERE [Success] = 1";
        var rows = await _context.ExecuteQueryAsync(query);
        return rows.Select(r => r["Version"]?.ToString() ?? string.Empty);
    }

    private static MigrationRecord MapRow(Dictionary<string, object> row) => new()
    {
        Id          = Convert.ToInt32(row["Id"], CultureInfo.InvariantCulture),
        Version     = row["Version"]?.ToString() ?? string.Empty,
        Description = row["Description"]?.ToString() ?? string.Empty,
        AppliedAt   = Convert.ToDateTime(row["AppliedAt"], CultureInfo.InvariantCulture),
        Success     = Convert.ToBoolean(row["Success"], CultureInfo.InvariantCulture),
        ErrorMessage = row.TryGetValue("ErrorMessage", out var err) && err != DBNull.Value
                       ? err?.ToString()
                       : null
    };
}
