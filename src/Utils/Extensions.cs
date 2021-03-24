// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Utils;

using System.Linq.Expressions;
using System.Reflection;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Extension methods for common operations
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Gets table name for entity type
    /// </summary>
    public static string GetTableName(this Type entityType)
    {
        var attr = entityType.GetCustomAttribute<TableAttribute>();
        return attr?.Name ?? entityType.Name + "s";
    }

    /// <summary>
    /// Gets table schema for entity type
    /// </summary>
    public static string GetTableSchema(this Type entityType)
    {
        var attr = entityType.GetCustomAttribute<TableAttribute>();
        return attr?.Schema ?? Constants.OrmConstants.DefaultSchema;
    }

    /// <summary>
    /// Gets column name for property
    /// </summary>
    public static string GetColumnName(this PropertyInfo property)
    {
        var attr = property.GetCustomAttribute<ColumnAttribute>();
        return attr?.Name ?? property.Name;
    }

    /// <summary>
    /// Gets all mapped columns for entity type
    /// </summary>
    public static List<PropertyInfo> GetMappedProperties(this Type entityType)
    {
        return entityType
            .GetProperties()
            .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null)
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .ToList();
    }

    /// <summary>
    /// Gets primary key property for entity type
    /// </summary>
    public static PropertyInfo? GetPrimaryKeyProperty(this Type entityType)
    {
        return entityType
            .GetProperties()
            .FirstOrDefault(p =>
            {
                var attr = p.GetCustomAttribute<ColumnAttribute>();
                return attr?.IsPrimaryKey == true;
            });
    }

    /// <summary>
    /// Creates a copy of entity with new ID
    /// </summary>
    public static T CloneWithNewId<T>(this T entity) where T : BaseEntity, new()
    {
        var copy = new T();
        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.Name == "Id")
                continue;

            if (prop.CanRead && prop.CanWrite)
                prop.SetValue(copy, prop.GetValue(entity));
        }
        return copy;
    }

    /// <summary>
    /// Converts entity to dictionary
    /// </summary>
    public static Dictionary<string, object> ToDictionary<T>(this T entity) where T : BaseEntity
    {
        var dict = new Dictionary<string, object>();
        foreach (var prop in typeof(T).GetProperties())
        {
            var value = prop.GetValue(entity);
            if (value != null)
                dict[prop.Name] = value;
        }
        return dict;
    }

    /// <summary>
    /// Checks if property has changed
    /// </summary>
    public static bool HasPropertyChanged<T>(this T entity, string propertyName, object originalValue) where T : BaseEntity
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property == null)
            return false;

        var currentValue = property.GetValue(entity);
        return !Equals(currentValue, originalValue);
    }

    /// <summary>
    /// Gets expression member name
    /// </summary>
    public static string GetMemberName<T, TProperty>(this Expression<Func<T, TProperty>> expression)
    {
        return expression.Body switch
        {
            MemberExpression me => me.Member.Name,
            UnaryExpression { Operand: MemberExpression me } => me.Member.Name,
            _ => throw new ArgumentException("Invalid expression")
        };
    }

    /// <summary>
    /// Paginates list
    /// </summary>
    public static (List<T> Items, int TotalCount) Paginate<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var list = source.ToList();
        var total = list.Count;
        var items = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (items, total);
    }

    /// <summary>
    /// Safely applies filter
    /// </summary>
    public static IQueryable<T> SafeWhere<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>>? predicate)
    {
        return predicate != null ? source.Where(predicate) : source;
    }

    /// <summary>
    /// Applies sorting
    /// </summary>
    public static IQueryable<T> ApplySort<T, TKey>(
        this IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector,
        bool ascending = true)
    {
        return ascending
            ? source.OrderBy(keySelector)
            : source.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Converts to JSON string representation
    /// </summary>
    public static string ToJsonString<T>(this T entity) where T : BaseEntity
    {
        var dict = entity.ToDictionary();
        var pairs = dict.Select(kvp => $"\"{kvp.Key}\":\"{kvp.Value}\"");
        return "{" + string.Join(",", pairs) + "}";
    }

    /// <summary>
    /// Checks if value is null or empty
    /// </summary>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Gets age in years
    /// </summary>
    public static int GetAgeInYears(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    /// <summary>
    /// Formats currency value
    /// </summary>
    public static string FormatCurrency(this decimal amount, string? currencySymbol = null)
    {
        return $"{currencySymbol ?? "$"}{amount:N2}";
    }
}
