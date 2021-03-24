#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Result wrapper for operation outcomes
/// </summary>
public abstract record Result
{
    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;

    public sealed record Success(object? Data = null) : Result;
    public sealed record Failure(string Message, string? ErrorCode = null, Exception? Exception = null) : Result;

    public static Result Ok(object? data = null) => new Success(data);
    public static Result Fail(string message, string? errorCode = null, Exception? exception = null) =>
        new Failure(message, errorCode, exception);

    public static Result<T> Ok<T>(T data) => new Result<T>.Success(data);
    public static Result<T> Fail<T>(string message, string? errorCode = null, Exception? exception = null) =>
        new Result<T>.Failure(message, errorCode, exception);
}

/// <summary>
/// Generic result wrapper for typed operations
/// </summary>
public abstract record Result<T>
{
    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;

    public sealed record Success(T Data) : Result<T>;
    public sealed record Failure(string Message, string? ErrorCode = null, Exception? Exception = null) : Result<T>;

    public static Result<T> Ok(T data) => new Success(data);
    public static Result<T> Fail(string message, string? errorCode = null, Exception? exception = null) =>
        new Failure(message, errorCode, exception);

    public TOut Match<TOut>(
        Func<T, TOut> onSuccess,
        Func<string, TOut> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Data),
            Failure f => onFailure(f.Message),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    public Task<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSuccess,
        Func<string, Task<TOut>> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Data),
            Failure f => onFailure(f.Message),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> map) =>
        this switch
        {
            Success s => Result<TOut>.Ok(await map(s.Data)),
            Failure f => Result<TOut>.Fail(f.Message, f.ErrorCode, f.Exception),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    public Result<TOut> Map<TOut>(Func<T, TOut> map) =>
        this switch
        {
            Success s => Result<TOut>.Ok(map(s.Data)),
            Failure f => Result<TOut>.Fail(f.Message, f.ErrorCode, f.Exception),
            _ => throw new InvalidOperationException("Unknown result type")
        };

    public Result<T> Tap(Action<T> action) =>
        this switch
        {
            Success s => { action(s.Data); return this; },
            _ => this
        };

    public async Task<Result<T>> TapAsync(Func<T, Task> action) =>
        this switch
        {
            Success s => { await action(s.Data); return this; },
            _ => this
        };
}

/// <summary>
/// Pagination result with metadata
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public int StartIndex => (PageNumber - 1) * PageSize + 1;
    public int EndIndex => Math.Min(StartIndex + Items.Count - 1, TotalCount);

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new(Items.Select(map).ToList(), PageNumber, PageSize, TotalCount);

    public override string ToString() =>
        $"Page {PageNumber} of {TotalPages} (Items {StartIndex}-{EndIndex} of {TotalCount})";
}

/// <summary>
/// Batch operation result
/// </summary>
public record BatchOperationResult(
    int TotalProcessed,
    int SuccessCount,
    int FailureCount,
    List<(int Index, string Error)> Failures)
{
    public double SuccessRate => TotalProcessed > 0 ? (double)SuccessCount / TotalProcessed * 100 : 0;
    public bool HasFailures => Failures.Count > 0;

    public override string ToString() =>
        $"Batch: {SuccessCount}/{TotalProcessed} successful ({SuccessRate:F2}%)";
}

/// <summary>
/// Operation statistics
/// </summary>
public record OperationStatistics(
    int TotalOperations,
    int SuccessCount,
    int FailureCount,
    long ElapsedMilliseconds)
{
    public double SuccessRate => TotalOperations > 0 ? (double)SuccessCount / TotalOperations * 100 : 0;
    public double OperationsPerSecond => ElapsedMilliseconds > 0 ? TotalOperations / (ElapsedMilliseconds / 1000.0) : 0;

    public override string ToString() =>
        $"Stats: {SuccessCount}/{TotalOperations} ops in {ElapsedMilliseconds}ms ({OperationsPerSecond:F2} ops/sec)";
}
