# INotificationService

A service interface for sending and tracking notifications via email, SMS, and push notifications, with support for templating, retries, and tagging.

## API

### `public NotificationService`

Constructor for the notification service. Initializes a new instance with default settings.

### `public async Task SendEmailAsync`

Sends an email notification asynchronously.

**Parameters:**
- None

**Return value:**
- `Task`: A task representing the asynchronous operation.

**Exceptions:**
- Throws `ArgumentNullException` if `Recipient` or `Body` is null.
- Throws `InvalidOperationException` if `Type` is not `NotificationType.Email`.

### `public async Task SendSmsAsync`

Sends an SMS notification asynchronously.

**Parameters:**
- None

**Return value:**
- `Task`: A task representing the asynchronous operation.

**Exceptions:**
- Throws `ArgumentNullException` if `Recipient` or `Body` is null.
- Throws `InvalidOperationException` if `Type` is not `NotificationType.Sms`.

### `public async Task SendPushNotificationAsync`

Sends a push notification asynchronously.

**Parameters:**
- None

**Return value:**
- `Task`: A task representing the asynchronous operation.

**Exceptions:**
- Throws `ArgumentNullException` if `Recipient` or `Body` is null.
- Throws `InvalidOperationException` if `Type` is not `NotificationType.Push`.

### `public async Task ProcessQueueAsync`

Processes the notification queue asynchronously, attempting to send queued notifications.

**Parameters:**
- None

**Return value:**
- `Task`: A task representing the asynchronous operation.

**Exceptions:**
- None

### `public void RegisterTemplate`

Registers a notification template for a given type.

**Parameters:**
- `templateName` (`string`): The name of the template.
- `templateContent` (`string`): The content of the template.

**Return value:**
- None

**Exceptions:**
- Throws `ArgumentNullException` if `templateName` or `templateContent` is null.

### `public string? GetTemplate`

Retrieves a registered template by name.

**Parameters:**
- `templateName` (`string`): The name of the template to retrieve.

**Return value:**
- `string?`: The template content, or `null` if not found.

**Exceptions:**
- Throws `ArgumentNullException` if `templateName` is null.

### `public string Id`

Gets the unique identifier for the notification.

**Parameters:**
- None

**Return value:**
- `string`: The unique identifier.

### `public NotificationType Type`

Gets the type of the notification (e.g., email, SMS, push).

**Parameters:**
- None

**Return value:**
- `NotificationType`: The type of the notification.

### `public string Recipient`

Gets the recipient of the notification.

**Parameters:**
- None

**Return value:**
- `string`: The recipient address or identifier.

### `public string? Subject`

Gets the subject of the notification, if applicable.

**Parameters:**
- None

**Return value:**
- `string?`: The subject, or `null` if not applicable.

### `public string Body`

Gets the body content of the notification.

**Parameters:**
- None

**Return value:**
- `string`: The body content.

### `public NotificationStatus Status`

Gets the current status of the notification.

**Parameters:**
- None

**Return value:**
- `NotificationStatus`: The status of the notification.

### `public DateTime CreatedAt`

Gets the timestamp when the notification was created.

**Parameters:**
- None

**Return value:**
- `DateTime`: The creation timestamp.

### `public DateTime? SentAt`

Gets the timestamp when the notification was sent, if applicable.

**Parameters:**
- None

**Return value:**
- `DateTime?`: The sent timestamp, or `null` if not sent.

### `public string? Error`

Gets the error message, if the notification failed.

**Parameters:**
- None

**Return value:**
- `string?`: The error message, or `null` if no error occurred.

### `public int RetryCount`

Gets the number of retry attempts for the notification.

**Parameters:**
- None

**Return value:**
- `int`: The retry count.

### `public Dictionary<string, string> Tags`

Gets the collection of tags associated with the notification.

**Parameters:**
- None

**Return value:**
- `Dictionary<string, string>`: The tags dictionary.

## Usage

### Example 1: Sending an Email Notification
