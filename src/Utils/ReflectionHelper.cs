// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Provides utility methods for reflection operations including property access,
/// type inspection, attribute reading, and dynamic object creation.
/// Caches results for performance optimization in frequently-called paths.
/// </summary>
public static class ReflectionHelper
{
    private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = [];
    private static readonly Dictionary<(Type, string), PropertyInfo> PropertyInfoCache = [];

    /// <summary>
    /// Gets all public properties for a type with caching
    /// </summary>
    public static PropertyInfo[] GetProperties(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (!PropertyCache.TryGetValue(type, out var properties))
        {
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            PropertyCache[type] = properties;
        }

        return properties;
    }

    /// <summary>
    /// Gets a specific property by name with caching and case-insensitive lookup
    /// </summary>
    public static PropertyInfo? GetProperty(Type type, string propertyName)
    {
        if (type is null || string.IsNullOrEmpty(propertyName))
            return null;

        var key = (type, propertyName);

        if (!PropertyInfoCache.TryGetValue(key, out var property))
        {
            property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is not null)
                PropertyInfoCache[key] = property;
        }

        return property;
    }

    /// <summary>
    /// Gets the value of a property from an object instance
    /// </summary>
    public static object? GetPropertyValue(object instance, string propertyName)
    {
        if (instance is null)
            return null;

        var property = GetProperty(instance.GetType(), propertyName);
        if (property?.CanRead == true)
        {
            try
            {
                return property.GetValue(instance);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the value of a property on an object instance
    /// </summary>
    public static void SetPropertyValue(object instance, string propertyName, object? value)
    {
        if (instance is null)
            return;

        var property = GetProperty(instance.GetType(), propertyName);
        if (property?.CanWrite == true)
        {
            try
            {
                property.SetValue(instance, value);
            }
            catch
            {
                // Silently ignore set failures
            }
        }
    }

    /// <summary>
    /// Checks if a type is a nullable reference type (string, arrays, classes)
    /// </summary>
    public static bool IsNullableType(Type type)
    {
        if (type is null)
            return true;

        if (type.IsValueType)
        {
            return Nullable.GetUnderlyingType(type) is not null;
        }

        return true; // Reference types are nullable
    }

    /// <summary>
    /// Checks if a type is a simple type (primitive, string, decimal, etc)
    /// </summary>
    public static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
               type == typeof(DateTime) || type == typeof(Guid) || type == typeof(TimeSpan);
    }

    /// <summary>
    /// Gets all attributes of a specific type from a member
    /// </summary>
    public static T[] GetAttributes<T>(MemberInfo member) where T : Attribute
    {
        if (member is null)
            return [];

        return member.GetCustomAttributes(typeof(T), false).Cast<T>().ToArray();
    }

    /// <summary>
    /// Gets the first attribute of a specific type from a member
    /// </summary>
    public static T? GetAttribute<T>(MemberInfo member) where T : Attribute
    {
        return GetAttributes<T>(member).FirstOrDefault();
    }

    /// <summary>
    /// Checks if a type implements a generic interface
    /// Example: Checks if type implements IEnumerable&lt;T&gt;
    /// </summary>
    public static bool ImplementsGenericInterface(Type type, Type genericInterface)
    {
        if (type is null || genericInterface is null)
            return false;

        if (!genericInterface.IsGenericTypeDefinition)
            return type.GetInterfaces().Contains(genericInterface);

        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface);
    }

    /// <summary>
    /// Creates a new instance of a type using the default constructor
    /// </summary>
    public static object? CreateInstance(Type type)
    {
        if (type is null)
            return null;

        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a new instance of a type using the specified constructor parameters
    /// </summary>
    public static T? CreateInstance<T>(Type type, params object?[] args) where T : class
    {
        if (type is null)
            return null;

        try
        {
            return Activator.CreateInstance(type, args) as T;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the generic type arguments for a generic type
    /// Example: For List&lt;string&gt;, returns [typeof(string)]
    /// </summary>
    public static Type[] GetGenericArguments(Type type)
    {
        if (type is null)
            return [];

        return type.GetGenericArguments();
    }

    /// <summary>
    /// Gets the underlying type of a nullable type
    /// Example: For Nullable&lt;int&gt;, returns typeof(int)
    /// </summary>
    public static Type GetUnderlyingType(Type type)
    {
        if (type is null)
            return type;

        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Clears internal caches (useful for testing or memory optimization)
    /// </summary>
    public static void ClearCache()
    {
        PropertyCache.Clear();
        PropertyInfoCache.Clear();
    }
}
