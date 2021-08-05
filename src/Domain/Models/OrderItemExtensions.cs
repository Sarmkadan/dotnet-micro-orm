#nullable enable

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Extension methods for OrderItem providing additional functionality
/// </summary>
public static class OrderItemExtensions
{
    /// <summary>
    /// Calculates the effective price per unit after discount
    /// </summary>
    /// <param name="item">The order item</param>
    /// <returns>The effective price per unit after applying discount</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    public static decimal GetEffectiveUnitPrice(this OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var subtotal = item.UnitPrice * item.Quantity;
        var afterDiscount = subtotal - item.Discount;
        return afterDiscount > 0 && item.Quantity > 0 ? afterDiscount / item.Quantity : 0;
    }

    /// <summary>
    /// Determines if the order item qualifies for free shipping based on quantity thresholds
    /// </summary>
    /// <param name="item">The order item</param>
    /// <param name="freeShippingThreshold">Minimum quantity required for free shipping</param>
    /// <returns>True if quantity meets or exceeds threshold</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    public static bool QualifiesForFreeShipping(this OrderItem item, int freeShippingThreshold = 5)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.Quantity >= freeShippingThreshold;
    }

    /// <summary>
    /// Gets the discount percentage applied to this order item
    /// </summary>
    /// <param name="item">The order item</param>
    /// <returns>Discount percentage (0-100), or 0 if no discount</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    public static decimal GetDiscountPercentage(this OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var subtotal = item.UnitPrice * item.Quantity;
        return subtotal > 0 ? (item.Discount / subtotal) * 100 : 0;
    }

    /// <summary>
    /// Creates a formatted string representation of the order item for receipts/invoices
    /// </summary>
    /// <param name="item">The order item</param>
    /// <param name="includeTax">Whether to include tax information in the output</param>
    /// <returns>Formatted string representation</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    public static string ToReceiptString(this OrderItem item, bool includeTax = true)
    {
        ArgumentNullException.ThrowIfNull(item);

        var result = $"{item.Quantity}x {item.ProductName} @ {item.UnitPrice:C} = {item.GetSubtotal():C}";

        if (item.Discount > 0)
        {
            result += $"\n Discount: -{item.Discount:C} ({item.GetDiscountPercentage():F2}%)";
        }

        result += $"\n Total: {item.GetAfterDiscount():C}";

        if (includeTax && item.TaxAmount > 0)
        {
            result += $"\n Tax: +{item.TaxAmount:C} ({item.GetTaxRate():P1})";
        }

        result += $"\n Final: {item.LineTotal:C}";

        return result;
    }
}