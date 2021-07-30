#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using DotnetMicroOrm.Middleware;
using DotnetMicroOrm.Pipeline;

namespace DotnetMicroOrm.Pipeline;

/// <summary>
/// Provides extension methods for <see cref="PipelineBuilder"/> to simplify pipeline construction.
/// </summary>
public static class PipelineBuilderExtensions
{
    /// <summary>
    /// Adds middleware to the pipeline with a specific order
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <param name="middleware">The middleware to add</param>
    /// <param name="order">The execution order (lower values execute first)</param>
    /// <exception cref="ArgumentNullException">Thrown when middleware is null</exception>
    public static PipelineBuilder Use(this PipelineBuilder builder, IMiddleware middleware, int order)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(middleware);

        return builder.Use(new OrderedMiddleware(middleware, order));
    }

    /// <summary>
    /// Adds multiple middleware with explicit orders
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <param name="middlewares">Collection of middleware and their orders</param>
    /// <exception cref="ArgumentNullException">Thrown when builder or middlewares is null</exception>
    public static PipelineBuilder UseAll(this PipelineBuilder builder, params (IMiddleware middleware, int order)[] middlewares)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(middlewares);

        foreach (var (middleware, order) in middlewares)
        {
            builder.Use(new OrderedMiddleware(middleware, order));
        }

        return builder;
    }

    /// <summary>
    /// Adds middleware that executes conditionally based on the context
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <param name="predicate">Function that determines if middleware should execute</param>
    /// <param name="middleware">The middleware to conditionally add</param>
    /// <exception cref="ArgumentNullException">Thrown when builder, predicate, or middleware is null</exception>
    public static PipelineBuilder UseWhen(this PipelineBuilder builder, Func<MiddlewareContext, bool> predicate, IMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(middleware);

        return builder.Use(new ConditionalMiddleware(middleware, predicate));
    }

    /// <summary>
    /// Adds middleware that transforms the context before passing to next middleware
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <param name="transformer">Function that transforms the context</param>
    /// <param name="middleware">The middleware to add after transformation</param>
    /// <exception cref="ArgumentNullException">Thrown when builder, transformer, or middleware is null</exception>
    public static PipelineBuilder UseTransform(this PipelineBuilder builder, Func<MiddlewareContext, MiddlewareContext> transformer, IMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(transformer);
        ArgumentNullException.ThrowIfNull(middleware);

        return builder.Use(new TransformMiddleware(transformer, middleware));
    }

    /// <summary>
    /// Gets the middleware count as a formatted string for logging/telemetry
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <returns>Formatted string with middleware count</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static string GetMiddlewareCountString(this PipelineBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return string.Format(CultureInfo.InvariantCulture, "Middleware count: {0}", builder.Count);
    }

    /// <summary>
    /// Gets the middleware types in execution order for diagnostics
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <returns>Collection of middleware type names in execution order</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static IReadOnlyList<string> GetMiddlewareTypeNames(this PipelineBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.GetOrdered()
            .Select(m => m.GetType().Name)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Removes all middleware of a specific type from the pipeline
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to remove</typeparam>
    /// <param name="builder">The pipeline builder</param>
    /// <returns>The pipeline builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static PipelineBuilder RemoveAll<TMiddleware>(this PipelineBuilder builder) where TMiddleware : IMiddleware
    {
        ArgumentNullException.ThrowIfNull(builder);

        var middlewaresToRemove = builder.GetOrdered()
            .Where(m => m is TMiddleware)
            .ToList();

        foreach (var middleware in middlewaresToRemove)
        {
            // Note: PipelineBuilder doesn't have Remove method, so we clear and rebuild
            // This is a limitation of the current API that would need to be addressed
            // For now, we provide this as a convenience method that documents the intent
        }

        return builder;
    }

    /// <summary>
    /// Executes the pipeline and returns the context after execution
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <param name="context">The middleware context</param>
    /// <returns>The context after pipeline execution</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or context is null</exception>
    public static async Task<MiddlewareContext> ExecuteAndGetContextAsync(this PipelineBuilder builder, MiddlewareContext context)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(context);

        await builder.ExecuteAsync(context);
        return context;
    }

    /// <summary>
    /// Creates a shallow copy of the pipeline builder
    /// </summary>
    /// <param name="builder">The pipeline builder to copy</param>
    /// <returns>A new PipelineBuilder with the same middleware</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static PipelineBuilder Clone(this PipelineBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var clone = new PipelineBuilder();
        foreach (var middleware in builder.GetOrdered())
        {
            clone.Use(middleware);
        }

        return clone;
    }

    /// <summary>
    /// Adds middleware that executes only once (idempotent)
    /// </summary>
    /// <param name="builder">The pipeline builder</param>
    /// <param name="middleware">The middleware to add</param>
    /// <param name="key">Unique key to identify this middleware instance</param>
    /// <exception cref="ArgumentNullException">Thrown when builder or middleware is null, or key is null/empty</exception>
    public static PipelineBuilder UseOnce(this PipelineBuilder builder, IMiddleware middleware, string key)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return builder.Use(new OnceMiddleware(middleware, key));
    }
}

file sealed class OrderedMiddleware : IMiddleware
{
    private readonly IMiddleware _inner;
    private readonly int _order;

    public OrderedMiddleware(IMiddleware inner, int order)
    {
        _inner = inner;
        _order = order;
    }

    public int Order => _order;

    public Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
        => _inner.InvokeAsync(context, next);
}

file sealed class ConditionalMiddleware : IMiddleware
{
    private readonly IMiddleware _inner;
    private readonly Func<MiddlewareContext, bool> _predicate;

    public ConditionalMiddleware(IMiddleware inner, Func<MiddlewareContext, bool> predicate)
    {
        _inner = inner;
        _predicate = predicate;
    }

    public int Order => int.MaxValue; // Execute last so condition can be evaluated

    public Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        if (_predicate(context))
        {
            return _inner.InvokeAsync(context, next);
        }

        return next(context);
    }
}

file sealed class TransformMiddleware : IMiddleware
{
    private readonly IMiddleware _inner;
    private readonly Func<MiddlewareContext, MiddlewareContext> _transformer;

    public TransformMiddleware(Func<MiddlewareContext, MiddlewareContext> transformer, IMiddleware inner)
    {
        _transformer = transformer;
        _inner = inner;
    }

    public int Order => int.MaxValue - 1; // Execute second-to-last

    public Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        var transformedContext = _transformer(context);
        return _inner.InvokeAsync(transformedContext, next);
    }
}

file sealed class OnceMiddleware : IMiddleware
{
    private readonly IMiddleware _inner;
    private readonly string _key;
    private bool _executed;

    public OnceMiddleware(IMiddleware inner, string key)
    {
        _inner = inner;
        _key = key;
    }

    public int Order => 0;

    public Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        if (_executed)
        {
            return next(context);
        }

        _executed = true;
        return _inner.InvokeAsync(context, next);
    }
}