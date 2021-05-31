#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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
    /// <param name="order">The order to calculate weight for</param>
    /// <returns>Total weight in kilograms, or 0 if no items or products</returns>
    public static decimal GetTotalWeight(this Order order)
    {
        if (order?.Items == null || order.Items.Count == 0)
            return 0;

        // Assuming each product has a weight of 0.5kg for calculation purposes
        // In a real application, this would come from the Product entity
        const decimal defaultProductWeightKg = 0.5m;

        return order.Items.Sum(item => item.Quantity * defaultProductWeightKg);
    }

    /// <summary>
    /// Determines if the order is considered "urgent" based on order date and status
    /// </summary>
    /// <param name="order">The order to check</param>
    /// <param name="urgentDaysThreshold">Number of days to consider urgent (default: 2)</param>
    /// <returns>True if order is urgent, false otherwise</returns>
    public static bool IsUrgent(this Order order, int urgentDaysThreshold = 2)
    {
        if (order == null)
            return false;

        var daysSinceOrder = DateTime.UtcNow - order.OrderDate;

        return daysSinceOrder.TotalDays <= urgentDaysThreshold &&
               (order.Status == "Pending" || order.Status == "Confirmed");
    }

    /// <summary>
    /// Gets the formatted order details as a string for display purposes
    /// </summary>
    /// <param name="order">The order to format</param>
    /// <returns>Formatted string with order details</returns>
    public static string ToDisplayString(this Order order)
    {
        if (order == null)
            return "Order: null";

        var shippingAddress = order.ShippingAddress != null && order.ShippingAddress.Length > 30
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
    public static DateTime? GetEstimatedDeliveryDate(this Order order, int defaultDeliveryDays = 5)
    {
        if (order == null)
            return null;

        switch (order.Status)
        {
            case "Shipped" when order.ShippingDate.HasValue:
                return order.ShippingDate.Value.AddDays(defaultDeliveryDays);
            case "Confirmed" when !order.ShippingDate.HasValue:
                return order.OrderDate.AddDays(defaultDeliveryDays);
            case "Pending":
                return order.OrderDate.AddDays(defaultDeliveryDays);
            case "Delivered":
                return order.DeliveryDate;
            case "Cancelled":
                return null;
            default:
                return null;
        }
    }
}