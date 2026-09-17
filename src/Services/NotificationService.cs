#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Services;

/// <summary>
/// Service for sending notifications through multiple channels (email, SMS, push notifications).
/// Supports notification queuing, templating, and delivery tracking.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Queues an email notification for delivery.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The email body content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Queues an SMS notification for delivery.
    /// </summary>
    /// <param name="phoneNumber">The recipient phone number.</param>
    /// <param name="message">The SMS message content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Queues a push notification for delivery to a user.
    /// </summary>
    /// <param name="userId">The identifier of the recipient user.</param>
    /// <param name="title">The push notification title.</param>
    /// <param name="message">The push notification message content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendPushNotificationAsync(int userId, string title, string message);
}

/// <summary>
/// Default notification service implementation
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly Queue<Notification> _queue = new();
    private readonly Dictionary<string, string> _templates = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class
    /// and loads the default notification templates.
    /// </summary>
    public NotificationService()
    {
        InitializeTemplates();
    }

    /// <summary>
    /// Queues an email notification for delivery.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The email body content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="to"/>, <paramref name="subject"/>, or <paramref name="body"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="to"/> is empty or whitespace.</exception>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(body);

        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Email address cannot be empty", nameof(to));

        var notification = new Notification
        {
            Type = NotificationType.Email,
            Recipient = to,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow,
            Status = NotificationStatus.Queued
        };

        _queue.Enqueue(notification);
        Console.WriteLine($"Email queued for {to}: {subject}");

        // In production, would send via SMTP or email service
        await Task.Delay(10); // Simulate async operation
    }

    /// <summary>
    /// Queues an SMS notification for delivery.
    /// </summary>
    /// <param name="phoneNumber">The recipient phone number.</param>
    /// <param name="message">The SMS message content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="phoneNumber"/> or <paramref name="message"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="phoneNumber"/> or <paramref name="message"/> is empty or whitespace.</exception>
    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        var notification = new Notification
        {
            Type = NotificationType.Sms,
            Recipient = phoneNumber,
            Body = message,
            CreatedAt = DateTime.UtcNow,
            Status = NotificationStatus.Queued
        };

        _queue.Enqueue(notification);
        Console.WriteLine($"SMS queued for {phoneNumber}: {message[..Math.Min(50, message.Length)]}...");

        // In production, would use SMS provider (Twilio, SNS, etc)
        await Task.Delay(10);
    }

    /// <summary>
    /// Queues a push notification for delivery to a user.
    /// </summary>
    /// <param name="userId">The identifier of the recipient user.</param>
    /// <param name="title">The push notification title.</param>
    /// <param name="message">The push notification message content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="title"/> or <paramref name="message"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="title"/> is empty or whitespace.</exception>
    public async Task SendPushNotificationAsync(int userId, string title, string message)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        var notification = new Notification
        {
            Type = NotificationType.PushNotification,
            Recipient = userId.ToString(),
            Subject = title,
            Body = message,
            CreatedAt = DateTime.UtcNow,
            Status = NotificationStatus.Queued
        };

        _queue.Enqueue(notification);
        Console.WriteLine($"Push notification queued for user {userId}: {title}");

        // In production, would use push service (Firebase, APNS, etc)
        await Task.Delay(10);
    }

    /// <summary>
    /// Processes queued notifications (background job)
    /// </summary>
    public async Task ProcessQueueAsync()
    {
        while (_queue.Count > 0)
        {
            var notification = _queue.Dequeue();

            try
            {
                notification.Status = NotificationStatus.Sending;
                // Simulate sending
                await Task.Delay(50);
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;

                Console.WriteLine($"Notification sent: {notification.Type} to {notification.Recipient}");
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.Error = ex.Message;
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Registers a notification template
    /// </summary>
    public void RegisterTemplate(string name, string template)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(template);

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Template name cannot be empty", nameof(name));

        _templates[name] = template;
    }

    /// <summary>
    /// Gets a registered template
    /// </summary>
    public string? GetTemplate(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _templates.TryGetValue(name, out var template) ? template : null;
    }

    private void InitializeTemplates()
    {
        _templates["welcome_email"] = "Welcome to our service, {UserName}!";
        _templates["order_confirmation"] = "Your order #{OrderNumber} has been confirmed.";
        _templates["shipment_notification"] = "Your order is on its way. Tracking: {TrackingNumber}";
    }
}

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    Email,
    Sms,
    PushNotification,
    InApp
}

/// <summary>
/// Notification delivery status
/// </summary>
public enum NotificationStatus
{
    Queued,
    Sending,
    Sent,
    Failed,
    Bounced
}

/// <summary>
/// Represents a notification to be sent
/// </summary>
public sealed class Notification
{
    /// <summary>Gets or sets the unique identifier of the notification.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the type of the notification.</summary>
    public NotificationType Type { get; set; }

    /// <summary>Gets or sets the recipient of the notification.</summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>Gets or sets the subject of the notification.</summary>
    public string? Subject { get; set; }

    /// <summary>Gets or sets the body content of the notification.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets the current delivery status of the notification.</summary>
    public NotificationStatus Status { get; set; }

    /// <summary>Gets or sets the time the notification was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the time the notification was sent, if applicable.</summary>
    public DateTime? SentAt { get; set; }

    /// <summary>Gets or sets the error message if delivery failed.</summary>
    public string? Error { get; set; }

    /// <summary>Gets or sets the number of delivery attempts made.</summary>
    public int RetryCount { get; set; }

    /// <summary>Gets or sets the collection of tags associated with the notification.</summary>
    public Dictionary<string, string> Tags { get; set; } = [];
}
