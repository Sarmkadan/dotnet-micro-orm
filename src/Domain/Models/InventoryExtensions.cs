using System;
using System.Globalization;

namespace DotnetMicroOrm.Domain.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="Inventory"/> class.
    /// </summary>
    public static class InventoryExtensions
    {
        /// <summary>
        /// Determines whether the inventory requires restocking based on the minimum threshold.
        /// </summary>
        /// <param name="inventory">The inventory instance.</param>
        /// <returns><see langword="true"/> if restocking is required; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="inventory"/> is <see langword="null"/>.</exception>
        public static bool RequiresRestocking(this Inventory inventory)
        {
            ArgumentNullException.ThrowIfNull(inventory);
            return inventory.CurrentStock <= inventory.MinimumThreshold;
        }

        /// <summary>
        /// Generates a summary of the inventory's current state, including stock levels and dates.
        /// </summary>
        /// <param name="inventory">The inventory instance.</param>
        /// <returns>A formatted summary string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="inventory"/> is <see langword="null"/>.</exception>
        public static string GenerateInventorySummary(this Inventory inventory)
        {
            ArgumentNullException.ThrowIfNull(inventory);
            return $@"
Inventory Summary:
- Product ID: {inventory.ProductId}
- Current Stock: {inventory.CurrentStock}
- Reserved Stock: {inventory.ReservedStock}
- Available Stock: {inventory.CurrentStock - inventory.ReservedStock}
- Last Restock: {inventory.LastRestockDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "Never"}
- Last Count: {inventory.LastCountDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "Never"}
- Low Stock: {(inventory.IsLowStock() ? "Yes" : "No")}";
        }

        /// <summary>
        /// Updates the inventory's current stock and records the last count date.
        /// </summary>
        /// <param name="inventory">The inventory instance.</param>
        /// <param name="newStockCount">The new stock count to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="inventory"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="newStockCount"/> is negative.</exception>
        public static void UpdateStockCount(this Inventory inventory, int newStockCount)
        {
            ArgumentNullException.ThrowIfNull(inventory);
            if (newStockCount < 0)
                throw new ArgumentOutOfRangeException(nameof(newStockCount), "Stock count cannot be negative.");

            inventory.CurrentStock = newStockCount;
            inventory.LastCountDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Calculates the percentage of current stock that is reserved.
        /// </summary>
        /// <param name="inventory">The inventory instance.</param>
        /// <returns>The percentage of reserved stock, or 0 if current stock is zero.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="inventory"/> is <see langword="null"/>.</exception>
        public static double GetReservationPercentage(this Inventory inventory)
        {
            ArgumentNullException.ThrowIfNull(inventory);
            if (inventory.CurrentStock == 0)
                return 0;

            return (double)inventory.ReservedStock / inventory.CurrentStock * 100;
        }
    }
}
