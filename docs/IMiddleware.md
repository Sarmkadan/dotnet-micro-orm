# IMiddleware
The `IMiddleware` type in the `dotnet-micro-orm` project represents a middleware component that can be used to handle and process requests in a micro-ORM (Object-Relational Mapping) system. It provides a set of properties that contain information about the current request, such as the request ID, operation, request and response data, user authentication information, and any exceptions that may have occurred. This type is designed to be used in a pipeline-based architecture, where multiple middleware components can be chained together to process requests in a specific order.

## API
The `IMiddleware` type has the following public members:
* `RequestId`: a string that uniquely identifies the current request.
* `Operation`: a string that describes the operation being performed.
* `RequestData`: an object that contains the data associated with the current request.
* `ResponseData`: an object that contains the data associated with the response to the current request.
* `User`: an `AuthenticationInfo` object that contains information about the authenticated user, or null if no user is authenticated.
* `Exception`: an `Exception` object that represents any exception that occurred during the processing of the request, or null if no exception occurred.
* `StartTime`: a `DateTime` that represents the time at which the request was started.
* `Metadata`: a dictionary of string-object pairs that contains additional metadata about the request.
* `IsHandled`: a boolean that indicates whether the request has been handled.
* `UserId`: an integer that represents the ID of the authenticated user.
* `Username`: a string that represents the username of the authenticated user.
* `Email`: a string that represents the email address of the authenticated user.
* `Role`: a string that represents the role of the authenticated user.
* `AuthenticatedAt`: a `DateTime` that represents the time at which the user was authenticated.

## Usage
Here are two examples of how the `IMiddleware` type can be used:
```csharp
// Example 1: Logging middleware
public class LoggingMiddleware : IMiddleware
{
    public string RequestId { get; set; }
    public string Operation { get; set; }
    public object? RequestData { get; set; }
    public object? ResponseData { get; set; }
    public AuthenticationInfo? User { get; set; }
    public Exception? Exception { get; set; }
    public DateTime StartTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public bool IsHandled { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime AuthenticatedAt { get; set; }

    public void HandleRequest()
    {
        Console.WriteLine($"Request {RequestId} started at {StartTime}");
        // Log request data and user information
        Console.WriteLine($"Request data: {RequestData}");
        Console.WriteLine($"User: {Username} ({Email})");
    }
}

// Example 2: Authentication middleware
public class AuthenticationMiddleware : IMiddleware
{
    public string RequestId { get; set; }
    public string Operation { get; set; }
    public object? RequestData { get; set; }
    public object? ResponseData { get; set; }
    public AuthenticationInfo? User { get; set; }
    public Exception? Exception { get; set; }
    public DateTime StartTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public bool IsHandled { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime AuthenticatedAt { get; set; }

    public void HandleRequest()
    {
        if (User == null)
        {
            // Authenticate user
            User = new AuthenticationInfo { Username = "john", Email = "john@example.com" };
            AuthenticatedAt = DateTime.Now;
        }
        else
        {
            // User is already authenticated, proceed with request
        }
    }
}
```

## Notes
When using the `IMiddleware` type, it is important to note that the `IsHandled` property should be set to `true` once the request has been handled, to prevent further processing by subsequent middleware components. Additionally, the `Exception` property should be set to the exception that occurred during processing, if any. The `Metadata` dictionary can be used to store additional information about the request, such as request headers or query parameters. It is also important to note that the `IMiddleware` type is not thread-safe, and should not be shared across multiple threads. Each middleware component should be instantiated separately for each request.
