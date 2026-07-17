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
    /// Adds middleware to the pipeline with a specific order.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="middleware">The middleware to add.</param>
    /// <param name="order">The execution order (lower values execute first).</param>
    /// <returns>The pipeline builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="middleware"/> is <see langword="null"/>.</exception>
    public static PipelineBuilder Use(this PipelineBuilder builder, IMiddleware middleware, int order)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(middleware);

        return builder.Use(new OrderedMiddleware(middleware, order));
    }

    /// <summary>
    /// Adds multiple middleware with explicit orders.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="middlewares">Collection of middleware and their orders.</param>
    /// <returns>The pipeline builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="middlewares"/> is <see langword="null"/>.</exception>
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
    /// Adds middleware that executes conditionally based on the context.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="predicate">Function that determines if middleware should execute.</param>
    /// <param name="middleware">The middleware to conditionally add.</param>
    /// <returns>The pipeline builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/>, <paramref name="predicate"/>, or <paramref name="middleware"/> is <see langword="null"/>.</exception>
    public static PipelineBuilder UseWhen(this PipelineBuilder builder, Func<MiddlewareContext, bool> predicate, IMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(middleware);

        return builder.Use(new ConditionalMiddleware(middleware, predicate));
    }

    /// <summary>
    /// Adds middleware that transforms the context before passing to next middleware.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="transformer">Function that transforms the context.</param>
    /// <param name="middleware">The middleware to add after transformation.</param>
    /// <returns>The pipeline builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/>, <paramref name="transformer"/>, or <paramref name="middleware"/> is <see langword="null"/>.</exception>
    public static PipelineBuilder UseTransform(this PipelineBuilder builder, Func<MiddlewareContext, MiddlewareContext> transformer, IMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(transformer);
        ArgumentNullException.ThrowIfNull(middleware);

        return builder.Use(new TransformMiddleware(transformer, middleware));
    }

    /// <summary>
    /// Gets the middleware count as a formatted string for logging/telemetry.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <returns>Formatted string with middleware count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static string GetMiddlewareCountString(this PipelineBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return string.Format(CultureInfo.InvariantCulture, "Middleware count: {0}", builder.Count);
    }

    /// <summary>
    /// Gets the middleware types in execution order for diagnostics.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <returns>Collection of middleware type names in execution order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> GetMiddlewareTypeNames(this PipelineBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.GetOrdered()
            .Select(m => m.GetType().Name)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Removes all middleware of a specific type from the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to remove.</typeparam>
    /// <param name="builder">The pipeline builder.</param>
    /// <returns>The pipeline builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static PipelineBuilder RemoveAll<TMiddleware>(this PipelineBuilder builder) where TMiddleware : IMiddleware
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.RemoveWhere(m => m is TMiddleware);

        return builder;
    }

    /// <summary>
    /// Executes the pipeline and returns the context after execution.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="context">The middleware context.</param>
    /// <returns>The context after pipeline execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public static async Task<MiddlewareContext> ExecuteAndGetContextAsync(this PipelineBuilder builder, MiddlewareContext context)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(context);

        await builder.ExecuteAsync(context);
        return context;
    }

    /// <summary>
    /// Creates a shallow copy of the pipeline builder.
    /// </summary>
    /// <param name="builder">The pipeline builder to copy.</param>
    /// <returns>A new PipelineBuilder with the same middleware.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
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
    /// Adds middleware that executes only once (idempotent).
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="middleware">The middleware to add.</param>
    /// <param name="key">Unique key to identify this middleware instance.</param>
    /// <returns>The pipeline builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="middleware"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/> or empty.</exception>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderedMiddleware"/> class.
    /// </summary>
    /// <param name="inner">The inner middleware to wrap.</param>
    /// <param name="order">The execution order (lower values execute first).</param>
    public OrderedMiddleware(IMiddleware inner, int order)
    {
        _inner = inner;
        _order = order;
    }

    /// <summary>
    /// Gets the execution order of this middleware.
    /// </summary>
    public int Order => _order;

    /// <summary>
    /// Invokes the middleware with the given context and next delegate.
    /// </summary>
    /// <param name="context">The middleware context.</param>
    /// <param name="next">The next middleware delegate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
        => _inner.InvokeAsync(context, next);
}

file sealed class ConditionalMiddleware : IMiddleware
{
    private readonly IMiddleware _inner;
    private readonly Func<MiddlewareContext, bool> _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalMiddleware"/> class.
    /// </summary>
    /// <param name="inner">The inner middleware to conditionally execute.</param>
    /// <param name="predicate">The predicate function that determines if middleware should execute.</param>
    public ConditionalMiddleware(IMiddleware inner, Func<MiddlewareContext, bool> predicate)
    {
        _inner = inner;
        _predicate = predicate;
    }

    /// <summary>
    /// Gets the execution order of this middleware.
    /// </summary>
    /// <remarks>
    /// This middleware executes last (int.MaxValue) so the condition can be evaluated
    /// after all other middleware has had a chance to modify the context.
    /// </remarks>
    public int Order => int.MaxValue;

    /// <summary>
    /// Invokes the middleware with the given context and next delegate.
    /// </summary>
    /// <param name="context">The middleware context.</param>
    /// <param name="next">The next middleware delegate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformMiddleware"/> class.
    /// </summary>
    /// <param name="transformer">The function that transforms the context.</param>
    /// <param name="inner">The inner middleware to invoke with the transformed context.</param>
    public TransformMiddleware(Func<MiddlewareContext, MiddlewareContext> transformer, IMiddleware inner)
    {
        _transformer = transformer;
        _inner = inner;
    }

    /// <summary>
    /// Gets the execution order of this middleware.
    /// </summary>
    /// <remarks>
    /// This middleware executes second-to-last (int.MaxValue - 1) so it can transform the context
    /// before the final middleware executes.
    /// </remarks>
    public int Order => int.MaxValue - 1;

    /// <summary>
    /// Invokes the middleware with the given context and next delegate.
    /// </summary>
    /// <param name="context">The middleware context.</param>
    /// <param name="next">The next middleware delegate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="OnceMiddleware"/> class.
    /// </summary>
    /// <param name="inner">The inner middleware to execute only once.</param>
    /// <param name="key">The unique key to identify this middleware instance.</param>
    public OnceMiddleware(IMiddleware inner, string key)
    {
        _inner = inner;
        _key = key;
    }

    /// <summary>
    /// Gets the execution order of this middleware.
    /// </summary>
    /// <remarks>
    /// This middleware executes first (order 0) to ensure it runs before other middleware.
    /// </remarks>
    public int Order => 0;

    /// <summary>
    /// Invokes the middleware with the given context and next delegate.
    /// </summary>
    /// <param name="context">The middleware context.</param>
    /// <param name="next">The next middleware delegate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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