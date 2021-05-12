#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotnetMicroOrm.BackgroundJobs;

/// <summary>
/// Extension methods for <see cref="JobScheduler"/> providing additional scheduling and monitoring capabilities.
/// </summary>
public static class JobSchedulerExtensions
{
    /// <summary>
    /// Registers a background job with the specified configuration
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="job">The background job to register</param>
    /// <param name="interval">The interval at which the job should execute</param>
    /// <param name="enabled">Whether the job is enabled</param>
    /// <param name="runOnStartup">Whether to run the job immediately on startup</param>
    public static void Register(this JobScheduler scheduler, IBackgroundJob job, TimeSpan interval, bool enabled = true, bool runOnStartup = false)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (job is null)
            throw new ArgumentNullException(nameof(job));

        var config = new JobScheduleConfig
        {
            Enabled = enabled,
            Interval = interval,
            RunOnStartup = runOnStartup,
            MaxRetries = 3,
            ExecutionTimeout = TimeSpan.FromMinutes(5),
            RetryDelay = TimeSpan.FromSeconds(5)
        };

        scheduler.Register(job, config);
    }

    /// <summary>
    /// Registers a background job with cron expression scheduling
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="job">The background job to register</param>
    /// <param name="cronExpression">Cron expression (e.g., "0 * * * *" for every hour)</param>
    /// <param name="enabled">Whether the job is enabled</param>
    public static void Register(this JobScheduler scheduler, IBackgroundJob job, string cronExpression, bool enabled = true)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (job is null)
            throw new ArgumentNullException(nameof(job));

        if (string.IsNullOrWhiteSpace(cronExpression))
            throw new ArgumentException("Cron expression cannot be null or empty", nameof(cronExpression));

        var config = new JobScheduleConfig
        {
            Enabled = enabled,
            CronExpression = cronExpression,
            MaxRetries = 3,
            ExecutionTimeout = TimeSpan.FromMinutes(5),
            RetryDelay = TimeSpan.FromSeconds(5)
        };

        scheduler.Register(job, config);
    }

    /// <summary>
    /// Executes a job immediately and returns the execution result
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="jobId">The ID of the job to execute</param>
    /// <returns>Execution result with success status and error details</returns>
    public static async Task<JobExecutionResult> ExecuteJobAsync(this JobScheduler scheduler, string jobId)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

        if (!scheduler.TryGetJobConfig(jobId, out var job, out var config))
        {
            return new JobExecutionResult
            {
                JobId = jobId,
                Success = false,
                ErrorMessage = $"Job '{jobId}' not found in scheduler"
            };
        }

        return await scheduler.ExecuteJobAsync(job, config);
    }

    /// <summary>
    /// Gets the execution history for a specific job filtered by success status
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="jobId">The job ID to filter by</param>
    /// <param name="successfulOnly">Whether to return only successful executions</param>
    /// <returns>Filtered execution history</returns>
    public static IEnumerable<JobExecutionResult> GetExecutionHistory(this JobScheduler scheduler, string jobId, bool successfulOnly)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

        return scheduler.GetExecutionHistory(jobId)
            .Where(h => successfulOnly ? h.Success : true)
            .OrderByDescending(h => h.StartTime);
    }

    /// <summary>
    /// Gets the most recent successful execution for a job
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="jobId">The job ID to check</param>
    /// <returns>The most recent successful execution or null if none found</returns>
    public static JobExecutionResult? GetLastSuccessfulExecution(this JobScheduler scheduler, string jobId)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

        return scheduler.GetExecutionHistory(jobId)
            .Where(h => h.Success)
            .OrderByDescending(h => h.StartTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the most recent failed execution for a job
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="jobId">The job ID to check</param>
    /// <returns>The most recent failed execution or null if none found</returns>
    public static JobExecutionResult? GetLastFailedExecution(this JobScheduler scheduler, string jobId)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

        return scheduler.GetExecutionHistory(jobId)
            .Where(h => !h.Success)
            .OrderByDescending(h => h.StartTime)
            .FirstOrDefault();
    }

    /// <summary>
    /// Checks if a job has ever executed successfully
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="jobId">The job ID to check</param>
    /// <returns>True if the job has successful executions, false otherwise</returns>
    public static bool HasSuccessfulExecutions(this JobScheduler scheduler, string jobId)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

        return scheduler.GetExecutionHistory(jobId)
            .Any(h => h.Success);
    }

    /// <summary>
    /// Gets statistics about job executions
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <returns>Job execution statistics</returns>
    public static JobStatistics GetStatistics(this JobScheduler scheduler)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        var allExecutions = scheduler.GetRecentExecutions(int.MaxValue).ToList();
        var successful = allExecutions.Count(h => h.Success);
        var failed = allExecutions.Count - successful;
        var avgDuration = allExecutions.Any()
            ? TimeSpan.FromTicks((long)allExecutions.Average(h => h.Duration.Ticks))
            : TimeSpan.Zero;

        return new JobStatistics
        {
            TotalExecutions = allExecutions.Count,
            SuccessfulExecutions = successful,
            FailedExecutions = failed,
            SuccessRate = allExecutions.Count > 0 ? (double)successful / allExecutions.Count * 100 : 0,
            AverageExecutionTime = avgDuration,
            LastExecutionTime = allExecutions.Max(h => h.StartTime)
        };
    }

    /// <summary>
    /// Gets the count of registered jobs
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <returns>Number of registered jobs</returns>
    public static int GetRegisteredJobsCount(this JobScheduler scheduler)
    {
        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        // Using reflection to access the private _jobs field since we can't modify the original class
        var field = typeof(JobScheduler).GetField("_jobs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(scheduler) is Dictionary<string, (IBackgroundJob job, JobScheduleConfig config)> jobs)
        {
            return jobs.Count;
        }

        return 0;
    }

    /// <summary>
    /// Tries to get a job by its ID
    /// </summary>
    /// <param name="scheduler">The job scheduler instance</param>
    /// <param name="jobId">The job ID to find</param>
    /// <param name="job">Outputs the job if found</param>
    /// <param name="config">Outputs the job configuration if found</param>
    /// <returns>True if the job was found, false otherwise</returns>
    private static bool TryGetJobConfig(this JobScheduler scheduler, string jobId, out IBackgroundJob job, out JobScheduleConfig config)
    {
        job = null!;
        config = null!;

        if (scheduler is null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

        var field = typeof(JobScheduler).GetField("_jobs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(scheduler) is Dictionary<string, (IBackgroundJob job, JobScheduleConfig config)> jobs)
        {
            if (jobs.TryGetValue(jobId, out var jobConfig))
            {
                job = jobConfig.job;
                config = jobConfig.config;
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Represents statistics about job executions
/// </summary>
public class JobStatistics
{
    /// <summary>Total number of executions</summary>
    public int TotalExecutions { get; set; }

    /// <summary>Number of successful executions</summary>
    public int SuccessfulExecutions { get; set; }

    /// <summary>Number of failed executions</summary>
    public int FailedExecutions { get; set; }

    /// <summary>Success rate in percentage</summary>
    public double SuccessRate { get; set; }

    /// <summary>Average execution time</summary>
    public TimeSpan AverageExecutionTime { get; set; }

    /// <summary>Time of the last execution</summary>
    public DateTime? LastExecutionTime { get; set; }
}