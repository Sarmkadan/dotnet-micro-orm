// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Constants;

public static class OrmConstants
{
    public const string DefaultSchema = "dbo";
    public const int DefaultCommandTimeout = 30;
    public const int DefaultBatchSize = 1000;
    public const int MaxBatchSize = 10000;

    public static class ParameterPrefixes
    {
        public const string Sql = "@";
        public const string Oracle = ":";
        public const string MySql = "@";
        public const string PostgreSql = "@";
    }

    public static class Messages
    {
        public const string InvalidConnection = "Database connection is invalid or null";
        public const string EntityNotFound = "Entity with the specified key was not found";
        public const string InvalidBatchSize = "Batch size must be between 1 and {0}";
        public const string EntityAlreadyTracked = "Entity is already being tracked for changes";
        public const string UnitOfWorkNotActive = "Unit of work is not active";
        public const string InvalidPropertyType = "Property type {0} is not supported for mapping";
        public const string CommandAlreadyExecuting = "Another command is already executing";
    }

    public static class Sql
    {
        public const string SelectTemplate = "SELECT {0} FROM {1} WHERE {2}";
        public const string InsertTemplate = "INSERT INTO {0} ({1}) VALUES ({2})";
        public const string UpdateTemplate = "UPDATE {0} SET {1} WHERE {2}";
        public const string DeleteTemplate = "DELETE FROM {0} WHERE {1}";
    }
}

/// <summary>
/// Database provider types supported by the ORM
/// </summary>
public enum DatabaseProvider
{
    SqlServer = 0,
    PostgreSql = 1,
    MySql = 2,
    Oracle = 3,
    Sqlite = 4
}

/// <summary>
/// Entity state for change tracking
/// </summary>
public enum EntityState
{
    Detached = 0,
    Unchanged = 1,
    Added = 2,
    Modified = 3,
    Deleted = 4
}

/// <summary>
/// Isolation level for transactions
/// </summary>
public enum TransactionIsolationLevel
{
    ReadUncommitted = 0,
    ReadCommitted = 1,
    RepeatableRead = 2,
    Serializable = 3,
    Snapshot = 4
}

/// <summary>
/// Query join types
/// </summary>
public enum JoinType
{
    Inner = 0,
    Left = 1,
    Right = 2,
    Full = 3
}

/// <summary>
/// Sort direction for query results
/// </summary>
public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}

/// <summary>
/// Comparison operators for query filters
/// </summary>
public enum ComparisonOperator
{
    Equal = 0,
    NotEqual = 1,
    GreaterThan = 2,
    GreaterThanOrEqual = 3,
    LessThan = 4,
    LessThanOrEqual = 5,
    Like = 6,
    In = 7,
    NotIn = 8,
    Between = 9,
    IsNull = 10,
    IsNotNull = 11
}
