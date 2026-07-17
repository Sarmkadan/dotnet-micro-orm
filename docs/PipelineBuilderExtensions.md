# PipelineBuilderExtensions

PipelineBuilderExtensions provides a set of extension methods for configuring and manipulating middleware pipelines in the dotnet-micro-orm framework. These methods enable fluent registration, conditional execution, transformation, and introspection of middleware components within a pipeline, supporting modular and reusable data processing workflows.

## API

### `Use`
```csharp
public static PipelineBuilder Use(this PipelineBuilder builder, Type middlewareType)
```
Adds a middleware component to the pipeline by its type. The middleware is instantiated and executed in the order it was added.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to extend.  
- `middlewareType`: The Type of the middleware to add.  
**Returns**: The modified PipelineBuilder for method chaining.  
**Throws**: `ArgumentNullException` if `middlewareType` is null.  

---

### `UseAll`
```csharp
public static PipelineBuilder UseAll(this PipelineBuilder builder, params Type[] middlewareTypes)
```
Adds multiple middleware components to the pipeline in the order specified.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to extend.  
- `middlewareTypes`: An array of middleware Types to add.  
**Returns**: The modified PipelineBuilder for method chaining.  
**Throws**: `ArgumentNullException` if `middlewareTypes` is null.  

---

### `UseWhen`
```csharp
public static PipelineBuilder UseWhen(this PipelineBuilder builder, Func<bool> condition, Type middlewareType)
```
Adds a middleware component that executes only when the provided condition evaluates to true.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to extend.  
- `condition`: A function returning a boolean to determine execution.  
- `middlewareType`: The Type of the middleware to conditionally add.  
**Returns**: The modified PipelineBuilder for method chaining.  
**Throws**: `ArgumentNullException` if `condition` or `middlewareType` is null.  

---

### `UseTransform`
```csharp
public static PipelineBuilder UseTransform(this PipelineBuilder builder, Func<object, object> transformer)
```
Adds a middleware component that transforms the context object using the provided function.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to extend.  
- `transformer`: A function that accepts and returns an object to transform the context.  
**Returns**: The modified PipelineBuilder for method chaining.  
**Throws**: `ArgumentNullException` if `transformer` is null.  

---

### `GetMiddlewareCountString`
```csharp
public static string GetMiddlewareCountString(this PipelineBuilder builder)
```
Returns a string representation of the number of middleware components currently registered in the pipeline.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to inspect.  
**Returns**: A string indicating the count of middleware components.  

---

### `GetMiddlewareTypeNames`
```csharp
public static IReadOnlyList<string> GetMiddlewareTypeNames(this PipelineBuilder builder)
```
Returns the names of all middleware types registered in the pipeline.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to inspect.  
**Returns**: A read-only list of middleware type names.  

---

### `RemoveAll<TMiddleware>`
```csharp
public static PipelineBuilder RemoveAll<TMiddleware>(this PipelineBuilder builder)
```
Removes all middleware components of the specified type from the pipeline.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to modify.  
**Returns**: The modified PipelineBuilder for method chaining.  

---

### `ExecuteAndGetContextAsync`
```csharp
public static async Task<MiddlewareContext> ExecuteAndGetContextAsync(this PipelineBuilder builder, MiddlewareContext context)
```
Executes the pipeline with the provided context and returns the resulting MiddlewareContext after all middleware have been processed.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to execute.  
- `context`: The initial MiddlewareContext to pass through the pipeline.  
**Returns**: A Task resolving to the final MiddlewareContext.  
**Throws**: Exceptions thrown by middleware components during execution.  

---

### `Clone`
```csharp
public static PipelineBuilder Clone(this PipelineBuilder builder)
```
Creates a deep copy of the current pipeline configuration, including all registered middleware.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to clone.  
**Returns**: A new PipelineBuilder with identical middleware configuration.  

---

### `UseOnce`
```csharp
public static PipelineBuilder UseOnce(this PipelineBuilder builder, Type middlewareType)
```
Adds a middleware component that executes exactly once per pipeline invocation, regardless of how many times the pipeline is run.  
**Parameters**:  
- `builder`: The PipelineBuilder instance to extend.  
- `middlewareType`: The Type of the middleware to add.  
**Returns**: The modified PipelineBuilder for method chaining.  
**Throws**: `ArgumentNullException` if `middlewareType` is null.  

---

### `OrderedMiddleware`
```csharp
public class OrderedMiddleware : IMiddleware
```
Represents a middleware component that executes in a specific order relative to other middleware.  
**InvokeAsync**:  
```csharp
public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
```
Executes the middleware logic and invokes the next component in the pipeline.  

---

### `ConditionalMiddleware`
```csharp
public class ConditionalMiddleware : IMiddleware
```
Wraps middleware execution based on a runtime condition.  
**InvokeAsync**:  
```csharp
public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
```
Evaluates the condition and executes the wrapped middleware only if the condition is met.  

---

### `TransformMiddleware`
```csharp
public class TransformMiddleware : IMiddleware
```
Applies a transformation function to the context during pipeline execution.  
**InvokeAsync**:  
```csharp
public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
```
Transforms the context using the configured function before passing it to the next middleware.  

---

### `OnceMiddleware`
```csharp
public class OnceMiddleware : IMiddleware
```
Ensures the middleware logic is executed only once per pipeline invocation.  
**InvokeAsync**:  
```csharp
public Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
```
Tracks execution state to prevent multiple invocations.  

## Usage

### Example 1: Basic Pipeline Configuration
```csharp
var pipeline = new PipelineBuilder()
    .Use(typeof(LoggingMiddleware))
    .UseAll(typeof(ValidationMiddleware), typeof(AuthorizationMiddleware))
    .UseWhen(() => DateTime.Now.Hour < 12, typeof(MorningDiscountMiddleware));

var context = new MiddlewareContext();
await pipeline.ExecuteAndGetContextAsync(context);
```

### Example 2: Transform and Remove Middleware
```csharp
var pipeline = new PipelineBuilder()
    .UseTransform(ctx => ctx.Result = ctx.Result?.ToString()?.ToUpper())
    .Use(typeof(CachingMiddleware));

pipeline.RemoveAll<CachingMiddleware>();

var clonedPipeline = pipeline.Clone();
```

## Notes

- **Thread Safety**: PipelineBuilder instances are not thread-safe. Concurrent modifications or executions may lead to unpredictable behavior. External synchronization is required for multi-threaded scenarios.  
- **Middleware Execution Order**: Middleware added via `Use` and `UseAll` execute in the order they are registered. Conditional and transform middleware may alter this order dynamically.  
- **Clone Behavior**: The `Clone` method creates a new PipelineBuilder with the same middleware configuration. Modifications to the clone do not affect the original pipeline.  
- **RemoveAll Edge Case**: Calling `RemoveAll<TMiddleware>` when no middleware of the specified type exists results in no operation. No exception is thrown.  
- **UseOnce Semantics**: OnceMiddleware ensures idempotent execution within a single pipeline invocation. It does not persist state across separate invocations.
