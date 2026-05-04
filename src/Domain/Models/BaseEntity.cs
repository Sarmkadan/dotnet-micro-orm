// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Base class for all domain entities with common properties and validation
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Validates the entity state. Override in derived classes for specific validation rules.
    /// </summary>
    public virtual bool Validate(out List<string> errors)
    {
        errors = [];
        return true;
    }

    /// <summary>
    /// Performs pre-save operations like setting timestamps
    /// </summary>
    public virtual void PreSave()
    {
        var modifiedDateProperty = GetType().GetProperty("ModifiedDate");
        if (modifiedDateProperty?.CanWrite == true && modifiedDateProperty.PropertyType == typeof(DateTime?))
        {
            modifiedDateProperty.SetValue(this, DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Performs post-load operations
    /// </summary>
    public virtual void PostLoad() { }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        var thisKey = GetType().GetProperty("Id")?.GetValue(this);
        var otherKey = obj.GetType().GetProperty("Id")?.GetValue(other);

        return thisKey != null && thisKey.Equals(otherKey);
    }

    public override int GetHashCode()
    {
        var keyProperty = GetType().GetProperty("Id");
        return keyProperty?.GetValue(this)?.GetHashCode() ?? base.GetHashCode();
    }

    public override string ToString() => $"{GetType().Name} {{ Id = {GetType().GetProperty("Id")?.GetValue(this) ?? "null"} }}";
}
