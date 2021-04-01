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
    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string phoneNumber, string message);
    Task SendPushNotificationAsync(int userId, string title, string message);
}

/// <summary>
/// Default notification service implementation
/// </summary>
public class NotificationService : INotificationService
{
    private readonly Queue<Notification> _queue = new();
    private readonly Dictionary<string, string> _templates = [];

    public NotificationService()
    {
        InitializeTemplates();
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
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

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
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

    public async Task SendPushNotificationAsync(int userId, string title, string message)
    {
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
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Template name cannot be empty", nameof(name));

        _templates[name] = template;
    }

    /// <summary>
    /// Gets a registered template
    /// </summary>
    public string? GetTemplate(string name)
    {
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
public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public Dictionary<string, string> Tags { get; set; } = [];
}
