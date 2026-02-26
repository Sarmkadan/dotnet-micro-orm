#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Records audit trail for entity changes and system operations
/// </summary>
[Table("AuditLogs")]
public class sealed AuditLog : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("EntityType", IsNullable = false, MaxLength = 100)]
    [Indexed]
    public string EntityType { get; set; } = string.Empty;

    [Column("EntityId", IsNullable = false)]
    public int EntityId { get; set; }

    [Column("Action", IsNullable = false, MaxLength = 20)]
    public string Action { get; set; } = string.Empty;

    [Column("UserId")]
    public int? UserId { get; set; }

    [Column("Username", MaxLength = 50)]
    public string? Username { get; set; }

    [Column("OldValues")]
    public string? OldValues { get; set; }

    [Column("NewValues")]
    public string? NewValues { get; set; }

    [Column("ChangedProperties")]
    public string? ChangedProperties { get; set; }

    [Column("IPAddress", MaxLength = 45)]
    public string? IPAddress { get; set; }

    [Column("UserAgent")]
    public string? UserAgent { get; set; }

    [Column("Description")]
    public string? Description { get; set; }

    [Column("IsSuccessful", IsNullable = false)]
    public bool IsSuccessful { get; set; } = true;

    [Column("ErrorMessage")]
    public string? ErrorMessage { get; set; }

    [Column("Timestamp", IsNullable = false)]
    [Indexed]
    public DateTime Timestamp { get; set; }

    public AuditLog() { }

    public AuditLog(string entityType, int entityId, string action)
    {
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        Timestamp = DateTime.UtcNow;
    }

    public override bool Validate(out List<string> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(EntityType) || EntityType.Length > 100)
            errors.Add("Entity type is required and cannot exceed 100 characters");

        if (EntityId <= 0)
            errors.Add("Valid entity ID is required");

        if (string.IsNullOrWhiteSpace(Action) || !IsValidAction(Action))
            errors.Add("Valid action is required");

        return errors.Count == 0;
    }

    public static AuditLog CreateInsert(string entityType, int entityId, string? newValues = null, int? userId = null, string? username = null)
    {
        return new AuditLog(entityType, entityId, "INSERT")
        {
            NewValues = newValues,
            UserId = userId,
            Username = username
        };
    }

    public static AuditLog CreateUpdate(string entityType, int entityId, string? oldValues = null, string? newValues = null, string? changedProps = null, int? userId = null, string? username = null)
    {
        return new AuditLog(entityType, entityId, "UPDATE")
        {
            OldValues = oldValues,
            NewValues = newValues,
            ChangedProperties = changedProps,
            UserId = userId,
            Username = username
        };
    }

    public static AuditLog CreateDelete(string entityType, int entityId, string? oldValues = null, int? userId = null, string? username = null)
    {
        return new AuditLog(entityType, entityId, "DELETE")
        {
            OldValues = oldValues,
            UserId = userId,
            Username = username
        };
    }

    public void MarkAsSuccess(string? description = null)
    {
        IsSuccessful = true;
        ErrorMessage = null;
        Description = description;
    }

    public void MarkAsFailure(string errorMessage, string? description = null)
    {
        IsSuccessful = false;
        ErrorMessage = errorMessage;
        Description = description;
    }

    public void SetIpAndUserAgent(string? ipAddress, string? userAgent)
    {
        IPAddress = ipAddress;
        UserAgent = userAgent;
    }

    private static bool IsValidAction(string action) => action is "INSERT" or "UPDATE" or "DELETE" or "READ";
}
