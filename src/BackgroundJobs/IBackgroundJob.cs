#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.BackgroundJobs;

/// <summary>
/// Interface for background jobs that run asynchronously outside the request pipeline.
/// Supports scheduling, execution, and error handling with retry logic.
/// </summary>
public interface IBackgroundJob
{
    /// <summary>Unique identifier for this job type</summary>
    string JobId { get; }

    /// <summary>Human-readable name for the job</summary>
    string Name { get; }

    /// <summary>Description of what this job does</summary>
    string Description { get; }

    /// <summary>Executes the job logic</summary>
    Task ExecuteAsync();

    /// <summary>Determines if the job can run in the current context</summary>
    bool CanExecute();

    /// <summary>Called when the job fails (for logging/alerting)</summary>
    Task OnFailureAsync(Exception ex);
}

/// <summary>
/// Job execution result with timing and status information
/// </summary>
public sealed class JobExecutionResult
{
    /// <summary>Job identifier</summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>When execution started</summary>
    public DateTime StartTime { get; set; }

    /// <summary>How long the job took to execute</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Whether execution succeeded</summary>
    public bool Success { get; set; }

    /// <summary>Error message if execution failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Stack trace if execution failed</summary>
    public string? StackTrace { get; set; }

    /// <summary>Custom job output/result data</summary>
    public Dictionary<string, object> Output { get; set; } = [];
}

/// <summary>
/// Configuration for job scheduling
/// </summary>
public sealed class JobScheduleConfig
{
    /// <summary>Run immediately on startup</summary>
    public bool RunOnStartup { get; set; }

    /// <summary>Interval between job executions</summary>
    public TimeSpan? Interval { get; set; }

    /// <summary>Cron expression for scheduling (e.g., "0 2 * * *" for 2 AM daily)</summary>
    public string? CronExpression { get; set; }

    /// <summary>Maximum number of times to retry on failure</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Delay between retries</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Timeout for job execution</summary>
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Whether this job is enabled</summary>
    public bool Enabled { get; set; } = true;
}
