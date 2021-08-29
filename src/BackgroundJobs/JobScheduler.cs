#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Globalization;

namespace DotnetMicroOrm.BackgroundJobs;

/// <summary>
/// Schedules and executes background jobs with support for intervals, retries,
/// and execution history tracking. Thread-safe and designed for distributed use.
/// </summary>
public sealed class JobScheduler : IAsyncDisposable
{
    private readonly Dictionary<string, (IBackgroundJob job, JobScheduleConfig config)> _jobs = [];
    private readonly ConcurrentDictionary<string, Timer?> _timers = [];
    private readonly List<JobExecutionResult> _executionHistory = [];
    private readonly object _historyLock = new();
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
                await ExecuteJobAsync(job, config);
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
                        result.Duration = DateTime.UtcNow - result.StartTime;
                        RecordExecution(result);
                        return result;
                    }

                    await job.ExecuteAsync().WaitAsync(config.ExecutionTimeout);

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
        lock (_historyLock)
        {
            return _executionHistory
                .Where(h => h.JobId == jobId)
                .OrderByDescending(h => h.StartTime)
                .ToList();
        }
    }

    /// <summary>
    /// Gets recent execution history (last N entries)
    /// </summary>
    public IEnumerable<JobExecutionResult> GetRecentExecutions(int count = 100)
    {
        lock (_historyLock)
        {
            return _executionHistory
                .OrderByDescending(h => h.StartTime)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Clears execution history
    /// </summary>
    public void ClearHistory()
    {
        lock (_historyLock)
        {
            _executionHistory.Clear();
        }
    }

    private void ScheduleInterval(string jobId, IBackgroundJob job, JobScheduleConfig config)
    {
        if (!config.Interval.HasValue)
            return;

        var timer = new Timer(
            _ => RunJobSafely(job, config),
            null,
            config.Interval.Value,
            config.Interval.Value);

        SetTimer(jobId, timer);
    }

    private void ScheduleCron(string jobId, IBackgroundJob job, JobScheduleConfig config)
    {
        if (string.IsNullOrEmpty(config.CronExpression))
            return;

        var now = DateTime.UtcNow;
        var nextRunTime = GetNextOccurrence(config.CronExpression, now);

        if (nextRunTime is null)
            return;

        var delay = nextRunTime.Value - now;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        var timer = new Timer(
            _ =>
            {
                RunJobSafely(job, config, () => ScheduleCron(jobId, job, config));
            },
            null,
            delay,
            Timeout.InfiniteTimeSpan);

        SetTimer(jobId, timer);
    }

    private void SetTimer(string jobId, Timer timer)
    {
        _timers.AddOrUpdate(
            jobId,
            timer,
            (_, existing) =>
            {
                existing?.Dispose();
                return timer;
            });
    }

    private void RunJobSafely(IBackgroundJob job, JobScheduleConfig config, Action? continuation = null)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await ExecuteJobAsync(job, config);
            }
            catch (Exception ex)
            {
                RecordExecution(new JobExecutionResult
                {
                    JobId = job.JobId,
                    StartTime = DateTime.UtcNow,
                    Success = false,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
            finally
            {
                continuation?.Invoke();
            }
        });
    }

    /// <summary>
    /// Computes the next UTC occurrence of a five field cron expression
    /// (minute hour day-of-month month day-of-week) strictly after <paramref name="after"/>.
    /// Each field supports <c>*</c>, single values, comma separated lists,
    /// <c>a-b</c> ranges and <c>*/step</c> or <c>a-b/step</c> increments.
    /// </summary>
    /// <param name="cronExpression">The cron expression to evaluate</param>
    /// <param name="after">The instant to search from (exclusive)</param>
    /// <returns>The next matching UTC instant, or null when the expression is invalid or never matches</returns>
    public static DateTime? GetNextOccurrence(string cronExpression, DateTime after)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            return null;

        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            return null;

        var minutes = ParseField(parts[0], 0, 59);
        var hours = ParseField(parts[1], 0, 23);
        var daysOfMonth = ParseField(parts[2], 1, 31);
        var months = ParseField(parts[3], 1, 12);
        var daysOfWeek = ParseField(parts[4], 0, 6);

        if (minutes is null || hours is null || daysOfMonth is null || months is null || daysOfWeek is null)
            return null;

        // Start from the next whole minute after the reference instant.
        var candidate = new DateTime(after.Year, after.Month, after.Day, after.Hour, after.Minute, 0, DateTimeKind.Utc)
            .AddMinutes(1);

        // Four years of minutes is enough to cover any leap year pattern.
        var limit = candidate.AddYears(4);

        while (candidate < limit)
        {
            if (!months.Contains(candidate.Month))
            {
                candidate = new DateTime(candidate.Year, candidate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
                continue;
            }

            if (!daysOfMonth.Contains(candidate.Day) || !daysOfWeek.Contains((int)candidate.DayOfWeek))
            {
                candidate = candidate.Date.AddDays(1);
                continue;
            }

            if (!hours.Contains(candidate.Hour))
            {
                candidate = candidate.Date.AddHours(candidate.Hour + 1);
                continue;
            }

            if (!minutes.Contains(candidate.Minute))
            {
                candidate = candidate.AddMinutes(1);
                continue;
            }

            return candidate;
        }

        return null;
    }

    private static HashSet<int>? ParseField(string field, int min, int max)
    {
        if (string.IsNullOrWhiteSpace(field))
            return null;

        var values = new HashSet<int>();

        foreach (var part in field.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var step = 1;
            var range = part;

            var slash = part.IndexOf('/');
            if (slash >= 0)
            {
                range = part[..slash];
                if (!int.TryParse(part[(slash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out step) || step <= 0)
                    return null;
            }

            int rangeStart;
            int rangeEnd;

            if (range is "*")
            {
                rangeStart = min;
                rangeEnd = max;
            }
            else if (range.Contains('-'))
            {
                var bounds = range.Split('-');
                if (bounds.Length != 2
                    || !int.TryParse(bounds[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out rangeStart)
                    || !int.TryParse(bounds[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out rangeEnd))
                    return null;
            }
            else if (int.TryParse(range, NumberStyles.Integer, CultureInfo.InvariantCulture, out var single))
            {
                rangeStart = single;
                rangeEnd = slash >= 0 ? max : single;
            }
            else
            {
                return null;
            }

            if (rangeStart < min || rangeEnd > max || rangeStart > rangeEnd)
                return null;

            for (var value = rangeStart; value <= rangeEnd; value += step)
            {
                values.Add(value);
            }
        }

        return values.Count > 0 ? values : null;
    }

    private void RecordExecution(JobExecutionResult result)
    {
        lock (_historyLock)
        {
            _executionHistory.Add(result);

            // Keep only recent history
            if (_executionHistory.Count > _maxHistoryEntries)
            {
                _executionHistory.RemoveRange(0, _executionHistory.Count - _maxHistoryEntries);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _executionLock?.Dispose();
    }
}
