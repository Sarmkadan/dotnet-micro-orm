# PipelineBuilder

The `PipelineBuilder` class configures a linear sequence of middleware components that are executed in order when the pipeline is invoked. It enables fluent registration of middleware, inspection of the registered components, and creation of a delegate that can be invoked asynchronously.

## API

### Use
```csharp
public PipelineBuilder Use(Func<MiddlewareContext, Task> middleware)
```
Adds a middleware delegate to the end of the pipeline.  
- **Parameters**  
  - `middleware`: The asynchronous middleware to invoke. Must not be `null`.  
- **Return value**  
  - The same `PipelineBuilder` instance to allow chaining.  
- **Exceptions**  
  - `ArgumentNullException` if `middleware` is `null`.

### UseAll
```csharp
public PipelineBuilder UseAll(IEnumerable<Func<MiddlewareContext, Task>> middlewares)
```
Adds multiple middleware delegates to the end of the pipeline preserving their enumeration order.  
- **Parameters**  
  - `middlewares`: Sequence of middleware delegates to add. Must not be `null` and must not contain `null` elements.  
- **Return value**  
  - The same `PipelineBuilder` instance to allow chaining.  
- **Exceptions**  
  - `ArgumentNullException` if `middlewares` is `null`.  
  - `ArgumentException` if any element in `middlewares` is `null`.

### Build
```csharp
public Func<MiddlewareContext, Task> Build()
```
Creates a delegate that encapsulates the registered middleware pipeline.  
- **Parameters**  
  - None.  
- **Return value**  
  - A `Func<MiddlewareContext, Task>` that, when invoked, executes the middleware in registration order.  
- **Exceptions**  
  - `InvalidOperationException` if the pipeline contains no middleware.

### ExecuteAsync
```csharp
public Task ExecuteAsync(MiddlewareContext context)
```
Executes the pipeline immediately using the currently registered middleware.  
- **Parameters**  
  - `context`: The contextual object passed to each middleware. Must not be `null`.  
- **Return value**  
  - A `Task` that completes when all middleware have finished processing.  
- **Exceptions**  
  - `ArgumentNullException` if `context` is `null`.  
  - `InvalidOperationException` if the pipeline contains no middleware.

### Clear
```csharp
public void Clear()
```
Removes all middleware from the builder, resetting it to an empty state.  
- **Parameters**  
  - None.  
- **Return value**  
  - None.  
- **Exceptions**  
  - None.

### GetOrdered
```csharp
public IEnumerable<IMiddleware> GetOrdered()
```
Returns the middleware components in the order they will be executed.  
- **Parameters**  
  - None.  
- **Return value**  
  - An `IEnumerable<IMiddleware>` yielding the registered middleware. The sequence reflects the current registration order and is a snapshot; subsequent modifications to the builder do not affect the returned enumeration.  
- **Exceptions**  
  - None.

## Usage

```csharp
var builder = new PipelineBuilder();

builder.Use(async ctx =>
{
    // pre‑processing logic
    await ctx.Next(); // invoke next middleware
    // post‑processing logic
});

builder.UseAll(new[]
{
    async ctx => { /* middleware A */ await ctx.Next(); },
    async ctx => { /* middleware B */ await ctx.Next(); }
});

var pipeline = builder.Build();
await pipeline.Invoke(new MiddlewareContext());
```

```csharp
var builder = new PipelineBuilder();
builder.Use(async ctx => { await ctx.Next(); });

// Inspect the registered middleware before building
foreach var m in builder.GetOrdered()
{
    Console.WriteLine(m.GetType().Name);
}

// Execute the pipeline directly
await builder.ExecuteAsync(new MiddlewareContext());

// Reset for a new configuration
builder.Clear();
```

## Notes

- The builder is **not thread‑safe**. Concurrent calls to `Use`, `UseAll`, `Clear`, or `Build` from multiple threads may result in undefined behavior. External synchronization is required if the instance is shared.  
- `GetOrdered` returns a snapshot; modifications after the call do not alter the enumerated sequence.  
- Passing `null` for any middleware delegate or for the `MiddlewareContext` argument will throw `ArgumentNullException`.  
- Invoking `Build` or `ExecuteAsync` on an empty pipeline throws `InvalidOperationException` because there is no middleware to execute.  
- The returned delegate from `Build` captures the middleware list at the moment of invocation; later changes to the builder do not affect already‑created delegates.  
- Middleware is expected to call the supplied `next` delegate (typically accessed via `MiddlewareContext.Next`) to continue the pipeline; failure to do so will short‑circuit the pipeline, which is a valid usage pattern.
