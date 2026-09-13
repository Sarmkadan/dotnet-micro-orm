#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Services;

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Order service for order processing and management
/// </summary>
public sealed class OrderService : IAsyncDisposable
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IDatabaseContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderService"/> class.
    /// </summary>
    /// <param name="context">The database context used to access order data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public OrderService(IDatabaseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _orderRepository = new Repository<Order>(context);
        _orderItemRepository = new Repository<OrderItem>(context);
    }

    /// <summary>
    /// Creates a new order for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user placing the order.</param>
    /// <param name="shippingAddress">The shipping address for the order.</param>
    /// <returns>The created order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="shippingAddress"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is less than or equal to zero, or when <paramref name="shippingAddress"/> is invalid.</exception>
    public async Task<Order> CreateOrderAsync(int userId, string shippingAddress)
    {
        ArgumentNullException.ThrowIfNull(shippingAddress);

        if (userId <= 0)
            throw new ArgumentException("Invalid user ID");

        if (string.IsNullOrWhiteSpace(shippingAddress) || shippingAddress.Length < 10)
            throw new ArgumentException("Invalid shipping address");

        var order = new Order(userId, shippingAddress)
        {
            CreatedDate = DateTime.UtcNow
        };

        return await _orderRepository.AddAsync(order);
    }

    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>The order with the specified ID, or <c>null</c> if not found.</returns>
    public async Task<Order?> GetOrderAsync(int orderId)
    {
        return await _orderRepository.GetByIdAsync(orderId);
    }

    /// <summary>
    /// Retrieves all orders for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose orders to retrieve.</param>
    /// <returns>A list of orders belonging to the specified user, ordered by order date descending.</returns>
    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
    }

    /// <summary>
    /// Retrieves all orders with the specified status.
    /// </summary>
    /// <param name="status">The order status to filter by.</param>
    /// <returns>A list of orders matching the specified status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="status"/> is empty or consists only of white-space.</exception>
    public async Task<List<Order>> GetOrdersByStatusAsync(string status)
    {
        ArgumentNullException.ThrowIfNull(status);

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Invalid status");

        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Adds an item to an existing order.
    /// </summary>
    /// <param name="orderId">The ID of the order to add the item to.</param>
    /// <param name="productId">The ID of the product to add.</param>
    /// <param name="productName">The name of the product to add.</param>
    /// <param name="quantity">The quantity of the product to add.</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    /// <returns>The updated order with the added item.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="productName"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the order is not found, or when item validation fails.</exception>
    public async Task<Order> AddOrderItemAsync(int orderId, int productId, string productName, int quantity, decimal unitPrice)
    {
        ArgumentNullException.ThrowIfNull(productName);

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new InvalidOperationException("Order not found");

        var item = new OrderItem(orderId, productId, productName, quantity, unitPrice)
        {
            CreatedDate = DateTime.UtcNow
        };

        item.Validate(out var errors);
        if (errors.Count > 0)
            throw new InvalidOperationException($"Item validation failed: {string.Join(", ", errors)}");

        item.CalculateLineTotal();
        await _orderItemRepository.AddAsync(item);
        order.AddItem(item);

        return await _orderRepository.UpdateAsync(order);
    }

    /// <summary>
    /// Confirms a pending order.
    /// </summary>
    /// <param name="orderId">The ID of the order to confirm.</param>
    /// <returns>The confirmed order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the order is not found, is not pending, or has no items.</exception>
    public async Task<Order> ConfirmOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new InvalidOperationException("Order not found");

        if (order.Status != "Pending")
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (order.Items.Count == 0)
            throw new InvalidOperationException("Cannot confirm order without items");

        order.Status = "Confirmed";
        order.ModifiedDate = DateTime.UtcNow;
        return await _orderRepository.UpdateAsync(order);
    }

    /// <summary>
    /// Ships an order on the specified date.
    /// </summary>
    /// <param name="orderId">The ID of the order to ship.</param>
    /// <param name="shipDate">The date the order was shipped; defaults to the current UTC time.</param>
    /// <returns>The shipped order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the order is not found.</exception>
    public async Task<Order> ShipOrderAsync(int orderId, DateTime? shipDate = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new InvalidOperationException("Order not found");

        order.Ship(shipDate ?? DateTime.UtcNow);
        return await _orderRepository.UpdateAsync(order);
    }

    /// <summary>
    /// Marks an order as delivered.
    /// </summary>
    /// <param name="orderId">The ID of the order to mark as delivered.</param>
    /// <returns>The delivered order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the order is not found.</exception>
    public async Task<Order> DeliverOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new InvalidOperationException("Order not found");

        order.MarkAsDelivered();
        return await _orderRepository.UpdateAsync(order);
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to cancel.</param>
    /// <returns>The canceled order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the order is not found.</exception>
    public async Task<Order> CancelOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new InvalidOperationException("Order not found");

        order.Cancel();
        return await _orderRepository.UpdateAsync(order);
    }

    /// <summary>
    /// Retrieves all pending orders.
    /// </summary>
    /// <returns>A list of orders with a pending status.</returns>
    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        return await GetOrdersByStatusAsync("Pending");
    }

    /// <summary>
    /// Calculates the total revenue from delivered and shipped orders.
    /// </summary>
    /// <returns>The total revenue as a decimal value.</returns>
    public async Task<decimal> GetTotalRevenueAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.Status is "Delivered" or "Shipped")
                     .Sum(o => o.TotalAmount);
    }

    /// <summary>
    /// Retrieves orders placed within the specified date range.
    /// </summary>
    /// <param name="startDate">The inclusive start of the date range.</param>
    /// <param name="endDate">The inclusive end of the date range.</param>
    /// <returns>A list of orders placed within the date range, ordered by order date descending.</returns>
    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                     .OrderByDescending(o => o.OrderDate)
                     .ToList();
    }

    /// <summary>
    /// Releases the resources used by the order service.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
