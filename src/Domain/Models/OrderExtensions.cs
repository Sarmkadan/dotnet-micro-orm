#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Linq;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides useful extension methods for the Order class
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Calculates the total weight of all items in the order based on product weights
    /// </summary>
    /// <remarks>
    /// This is a simplified calculation that assumes a default weight per item.
    /// In a production application, this would use the actual Product.Weight property.
    /// </remarks>
    /// <param name="order">The order to calculate weight for</param>
    /// <returns>Total weight in kilograms, or 0 if order or items are null</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="order"/> is null</exception>
    public static decimal GetTotalWeight(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.Items is null || order.Items.Count == 0)
            return 0;

        // Default weight per item in kilograms
        // Note: In a real application, this should use Product.Weight property
        const decimal defaultProductWeightKg = 0.5m;

        return order.Items.Sum(item => item.Quantity * defaultProductWeightKg);
    }

    /// <summary>
    /// Determines if the order is considered "urgent" based on order date and status
    /// </summary>
    /// <param name="order">The order to check</param>
    /// <param name="urgentDaysThreshold">Number of days to consider urgent (default: 2)</param>
    /// <returns>True if order is urgent, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="order"/> is null</exception>
    public static bool IsUrgent(this Order order, int urgentDaysThreshold = 2)
    {
        ArgumentNullException.ThrowIfNull(order);

        var daysSinceOrder = DateTime.UtcNow - order.OrderDate;

        return daysSinceOrder.TotalDays <= urgentDaysThreshold &&
               (order.Status is "Pending" or "Confirmed");
    }

    /// <summary>
    /// Gets the formatted order details as a string for display purposes
    /// </summary>
    /// <param name="order">The order to format</param>
    /// <returns>Formatted string with order details</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="order"/> is null</exception>
    public static string ToDisplayString(this Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var shippingAddress = order.ShippingAddress?.Length > 30
            ? order.ShippingAddress.Substring(0, 30)
            : order.ShippingAddress ?? "N/A";

        var result = $"Order #{order.OrderNumber}\n" +
                     $"Date: {order.OrderDate:yyyy-MM-dd HH:mm}\n" +
                     $"Status: {order.Status}\n" +
                     $"Total: {order.TotalAmount:C}\n" +
                     $"Items: {order.Items?.Count ?? 0}\n" +
                     $"Shipping: {shippingAddress}\n" +
                     $"Created: {order.CreatedDate:yyyy-MM-dd}";

        if (order.ModifiedDate.HasValue)
        {
            result += $"\nModified: {order.ModifiedDate.Value:yyyy-MM-dd}";
        }

        return result;
    }

    /// <summary>
    /// Calculates the estimated delivery date based on current status and shipping date
    /// </summary>
    /// <param name="order">The order to calculate delivery date for</param>
    /// <param name="defaultDeliveryDays">Default days for delivery (default: 5)</param>
    /// <returns>Estimated delivery date, or null if not estimable</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="order"/> is null</exception>
    public static DateTime? GetEstimatedDeliveryDate(this Order order, int defaultDeliveryDays = 5)
    {
        ArgumentNullException.ThrowIfNull(order);

        return order.Status switch
        {
            "Shipped" when order.ShippingDate.HasValue => order.ShippingDate.Value.AddDays(defaultDeliveryDays),
            "Confirmed" when !order.ShippingDate.HasValue => order.OrderDate.AddDays(defaultDeliveryDays),
            "Pending" => order.OrderDate.AddDays(defaultDeliveryDays),
            "Delivered" => order.DeliveryDate,
            "Cancelled" => null,
            _ => null
        };
    }
}