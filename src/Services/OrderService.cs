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
public class OrderService : IAsyncDisposable
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IDatabaseContext _context;

    public OrderService(IDatabaseContext context)
    {
        _context = context;
        _orderRepository = new Repository<Order>(context);
        _orderItemRepository = new Repository<OrderItem>(context);
    }

    // Creates new order
    public async Task<Order> CreateOrderAsync(int userId, string shippingAddress)
    {
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

    // Gets order by id
    public async Task<Order?> GetOrderAsync(int orderId)
    {
        return await _orderRepository.GetByIdAsync(orderId);
    }

    // Gets user orders
    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
    }

    // Gets orders by status
    public async Task<List<Order>> GetOrdersByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Invalid status");

        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // Adds item to order
    public async Task<Order> AddOrderItemAsync(int orderId, int productId, string productName, int quantity, decimal unitPrice)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
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

    // Confirms order
    public async Task<Order> ConfirmOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        if (order.Status != "Pending")
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (order.Items.Count == 0)
            throw new InvalidOperationException("Cannot confirm order without items");

        order.Status = "Confirmed";
        order.ModifiedDate = DateTime.UtcNow;
        return await _orderRepository.UpdateAsync(order);
    }

    // Ships order
    public async Task<Order> ShipOrderAsync(int orderId, DateTime? shipDate = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        order.Ship(shipDate ?? DateTime.UtcNow);
        return await _orderRepository.UpdateAsync(order);
    }

    // Marks order as delivered
    public async Task<Order> DeliverOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        order.MarkAsDelivered();
        return await _orderRepository.UpdateAsync(order);
    }

    // Cancels order
    public async Task<Order> CancelOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        order.Cancel();
        return await _orderRepository.UpdateAsync(order);
    }

    // Gets pending orders
    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        return await GetOrdersByStatusAsync("Pending");
    }

    // Gets total revenue
    public async Task<decimal> GetTotalRevenueAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.Status is "Delivered" or "Shipped")
                     .Sum(o => o.TotalAmount);
    }

    // Gets orders by date range
    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                     .OrderByDescending(o => o.OrderDate)
                     .ToList();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
