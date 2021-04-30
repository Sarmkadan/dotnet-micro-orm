#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Middleware;

namespace DotnetMicroOrm.Pipeline;

/// <summary>
/// Builds a middleware pipeline for request processing.
/// Allows composing multiple middleware components in a specific order.
/// Executes middleware in FIFO order, except for error handling which executes first.
/// </summary>
public sealed class PipelineBuilder
{
    private readonly List<IMiddleware> _middlewares = [];

    /// <summary>
    /// Adds middleware to the pipeline
    /// </summary>
    public PipelineBuilder Use(IMiddleware middleware)
    {
        if (middleware is null)
            throw new ArgumentNullException(nameof(middleware));

        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>
    /// Adds multiple middleware to the pipeline at once
    /// </summary>
    public PipelineBuilder UseAll(params IMiddleware[] middlewares)
    {
        if (middlewares is null)
            throw new ArgumentNullException(nameof(middlewares));

        foreach (var middleware in middlewares)
        {
            Use(middleware);
        }

        return this;
    }

    /// <summary>
    /// Builds the pipeline and returns a delegate that executes all middleware
    /// </summary>
    public Func<MiddlewareContext, Task> Build()
    {
        // Sort middleware by order (lower order = execute first)
        var sortedMiddlewares = _middlewares.OrderBy(m => m.Order).ToList();

        return async (context) =>
        {
            int index = -1;

            Func<MiddlewareContext, Task> next = null!;
            next = async (ctx) =>
            {
                index++;

                if (index < sortedMiddlewares.Count)
                {
                    await sortedMiddlewares[index].InvokeAsync(ctx, next);
                }
            };

            await next(context);
        };
    }

    /// <summary>
    /// Executes the pipeline with a specific context
    /// </summary>
    public async Task ExecuteAsync(MiddlewareContext context)
    {
        var pipeline = Build();
        await pipeline(context);
    }

    /// <summary>
    /// Gets the number of middleware in the pipeline
    /// </summary>
    public int Count => _middlewares.Count;

    /// <summary>
    /// Clears all middleware from the pipeline
    /// </summary>
    public void Clear()
    {
        _middlewares.Clear();
    }

    /// <summary>
    /// Gets a list of middleware in execution order
    /// </summary>
    public IEnumerable<IMiddleware> GetOrdered()
    {
        return _middlewares.OrderBy(m => m.Order);
    }
}
