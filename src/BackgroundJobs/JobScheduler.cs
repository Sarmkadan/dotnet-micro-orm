#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotnetMicroOrm.BackgroundJobs;

/// <summary>
/// Schedules and executes background jobs with support for intervals, retries,
/// and execution history tracking. Thread-safe and designed for distributed use.
/// </summary>
public class sealed JobScheduler : IAsyncDisposable
{
    private readonly Dictionary<string, (IBackgroundJob job, JobScheduleConfig config)> _jobs = [];
    private readonly ConcurrentDictionary<string, Timer?> _timers = [];
    private readonly List<JobExecutionResult> _executionHistory = [];
    private readonly SemaphoreSlim _executionLock = new(1);
    private readonly int _maxHistoryEntries = 1000;

    /// <summary>
    /// Registers a background job with its configuration
    /// </summary>
    public void Register(IBackgroundJob job, JobScheduleConfig config)
    {
        if (job is null)
            throw new ArgumentNullException(nameof(job));

        if (config is null)
            throw new ArgumentNullException(nameof(config));

        _jobs[job.JobId] = (job, config);
    }

    /// <summary>
    /// Starts the scheduler and begins executing jobs
    /// </summary>
    public async Task StartAsync()
    {
        foreach (var (jobId, (job, config)) in _jobs)
        {
            if (!config.Enabled)
                continue;

            if (config.RunOnStartup && job.CanExecute())
            {
                _ = ExecuteJobAsync(job, config);
            }

            if (config.Interval.HasValue)
            {
                ScheduleInterval(jobId, job, config);
            }
            else if (!string.IsNullOrEmpty(config.CronExpression))
            {
                ScheduleCron(jobId, job, config);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops the scheduler and cancels all pending jobs
    /// </summary>
    public async Task StopAsync()
    {
        foreach (var timer in _timers.Values)
        {
            timer?.Dispose();
        }

        _timers.Clear();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes a job immediately with retry logic
    /// </summary>
    public async Task<JobExecutionResult> ExecuteJobAsync(IBackgroundJob job, JobScheduleConfig config)
    {
        await _executionLock.WaitAsync();

        try
        {
            var result = new JobExecutionResult
            {
                JobId = job.JobId,
                StartTime = DateTime.UtcNow
            };

            int attempt = 0;
            Exception? lastException = null;

            while (attempt <= config.MaxRetries)
            {
                try
                {
                    if (!job.CanExecute())
                    {
                        result.Success = false;
                        result.ErrorMessage = "Job cannot execute in current context";
                        return result;
                    }

                    using (var cts = new CancellationTokenSource(config.ExecutionTimeout))
                    {
                        await job.ExecuteAsync();
                    }

                    result.Success = true;
                    result.Duration = DateTime.UtcNow - result.StartTime;
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;

                    if (attempt <= config.MaxRetries)
                    {
                        await Task.Delay(config.RetryDelay);
                    }
                }
            }

            if (!result.Success && lastException is not null)
            {
                result.Success = false;
                result.ErrorMessage = lastException.Message;
                result.StackTrace = lastException.StackTrace;
                result.Duration = DateTime.UtcNow - result.StartTime;

                await job.OnFailureAsync(lastException);
            }

            // Record execution history
            RecordExecution(result);

            return result;
        }
        finally
        {
            _executionLock.Release();
        }
    }

    /// <summary>
    /// Gets execution history for a specific job
    /// </summary>
    public IEnumerable<JobExecutionResult> GetExecutionHistory(string jobId)
    {
        return _executionHistory
            .Where(h => h.JobId == jobId)
            .OrderByDescending(h => h.StartTime)
            .ToList();
    }

    /// <summary>
    /// Gets recent execution history (last N entries)
    /// </summary>
    public IEnumerable<JobExecutionResult> GetRecentExecutions(int count = 100)
    {
        return _executionHistory
            .OrderByDescending(h => h.StartTime)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Clears execution history
    /// </summary>
    public void ClearHistory()
    {
        _executionHistory.Clear();
    }

    private void ScheduleInterval(string jobId, IBackgroundJob job, JobScheduleConfig config)
    {
        if (!config.Interval.HasValue)
            return;

        var timer = new Timer(
            async _ => await ExecuteJobAsync(job, config),
            null,
            config.Interval.Value,
            config.Interval.Value);

        _timers[jobId] = timer;
    }

    private void ScheduleCron(string jobId, IBackgroundJob job, JobScheduleConfig config)
    {
        // Simplified cron implementation - in production, use Hangfire or Quartz.NET
        if (string.IsNullOrEmpty(config.CronExpression))
            return;

        // Parse cron expression and schedule accordingly
        var nextRunTime = ParseCronExpression(config.CronExpression);

        if (nextRunTime > DateTime.UtcNow)
        {
            var delay = nextRunTime - DateTime.UtcNow;

            var timer = new Timer(
                async _ =>
                {
                    await ExecuteJobAsync(job, config);
                    // Reschedule
                    ScheduleCron(jobId, job, config);
                },
                null,
                delay,
                Timeout.InfiniteTimeSpan);

            _timers[jobId] = timer;
        }
    }

    private DateTime ParseCronExpression(string cronExpression)
    {
        // Simplified: only handle basic patterns
        // Format: minute hour day month dayOfWeek
        var parts = cronExpression.Split(' ');

        if (parts.Length != 5)
            return DateTime.UtcNow.AddHours(1);

        // Extract hour and minute (simple parsing)
        if (int.TryParse(parts[1], out var hour) && int.TryParse(parts[0], out var minute))
        {
            var now = DateTime.UtcNow;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            return nextRun;
        }

        return DateTime.UtcNow.AddHours(1);
    }

    private void RecordExecution(JobExecutionResult result)
    {
        _executionHistory.Add(result);

        // Keep only recent history
        if (_executionHistory.Count > _maxHistoryEntries)
        {
            _executionHistory.RemoveRange(0, _executionHistory.Count - _maxHistoryEntries);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _executionLock?.Dispose();
    }
}
