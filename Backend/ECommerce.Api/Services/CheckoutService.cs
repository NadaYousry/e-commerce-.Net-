using System.ComponentModel.DataAnnotations;
using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Services;

public sealed record CheckoutAddressRequest(
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
    string FullName,
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(ValidationRules.PhonePattern, ErrorMessage = "Enter a valid phone number (7–20 digits, optional leading +).")]
    string PhoneNumber,
    [Required(ErrorMessage = "Country is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Country must be between 2 and 100 characters.")]
    string Country,
    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters.")]
    string City,
    [Required(ErrorMessage = "Street address is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Street address must be between 3 and 200 characters.")]
    string StreetAddress,
    [StringLength(100, ErrorMessage = "Building / apartment must be 100 characters or fewer.")]
    string? BuildingApartmentDetails);

public sealed record CheckoutLineRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Product id is invalid.")]
    int ProductId,
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
    int Quantity);

public sealed record CheckoutRequest(
    [Required(ErrorMessage = "Your cart is empty.")]
    [MinLength(1, ErrorMessage = "Your cart is empty.")]
    IReadOnlyCollection<CheckoutLineRequest> Items,
    [Required(ErrorMessage = "Delivery address is required.")]
    CheckoutAddressRequest Address,
    [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Choose a valid payment method.")]
    PaymentMethod PaymentMethod,
    bool? MockCardSucceeded = null);

public sealed record OrderItemSummary(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record CheckoutResult(
    int OrderId,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    decimal Subtotal,
    string CurrencyCode,
    string FullName,
    string PhoneNumber,
    string Country,
    string City,
    string StreetAddress,
    string? BuildingApartmentDetails,
    PaymentMethod PaymentMethod,
    IReadOnlyCollection<OrderItemSummary> Items);

public sealed record OrderDetailsResponse(
    int Id,
    string OrderStatus,
    string PaymentStatus,
    decimal Subtotal,
    string CurrencyCode,
    string FullName,
    string PhoneNumber,
    string Country,
    string City,
    string StreetAddress,
    string? BuildingApartmentDetails,
    string PaymentMethod,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<OrderItemSummary> Items);

public interface ICheckoutService
{
    Task<CheckoutResult> CheckoutAsync(Guid userId, CheckoutRequest request);
    Task<OrderDetailsResponse?> GetOrderForUserAsync(Guid userId, int orderId);
    Task<IReadOnlyCollection<OrderDetailsResponse>> GetOrdersForUserAsync(Guid userId);
}

public sealed class CheckoutService(ECommerceDbContext dbContext) : ICheckoutService
{
    public async Task<CheckoutResult> CheckoutAsync(Guid userId, CheckoutRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("At least one item is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Address.FullName) ||
            string.IsNullOrWhiteSpace(request.Address.PhoneNumber) ||
            string.IsNullOrWhiteSpace(request.Address.Country) ||
            string.IsNullOrWhiteSpace(request.Address.City) ||
            string.IsNullOrWhiteSpace(request.Address.StreetAddress))
        {
            throw new ArgumentException("Delivery address is incomplete.");
        }

        var normalizedLines = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
            .ToList();

        if (normalizedLines.Any(item => item.Quantity <= 0))
        {
            throw new ArgumentException("Product quantities must be positive.");
        }

        if (normalizedLines.Count != request.Items.Count)
        {
            throw new ArgumentException("Each product can only appear once in a checkout request.");
        }

        if (request.PaymentMethod == PaymentMethod.MockCard && request.MockCardSucceeded is null)
        {
            throw new ArgumentException("Mock-card outcome is required in development.");
        }

        var productIds = normalizedLines.Select(item => item.ProductId).ToList();
        var products = await dbContext.Products
            .Where(product => productIds.Contains(product.Id) && product.IsActive)
            .ToListAsync();

        var missingProductIds = productIds.Except(products.Select(product => product.Id)).ToList();
        if (missingProductIds.Count != 0)
        {
            throw new InvalidOperationException($"One or more products are missing or inactive: {string.Join(", ", missingProductIds)}");
        }

        var processedItems = new List<(Product Product, int Quantity, decimal UnitPrice, decimal LineTotal)>();
        foreach (var line in normalizedLines)
        {
            var product = products.Single(item => item.Id == line.ProductId);
            if (line.Quantity > product.StockQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Requested {line.Quantity}, available {product.StockQuantity}.");
            }

            var unitPrice = product.Price;
            var lineTotal = unitPrice * line.Quantity;
            processedItems.Add((product, line.Quantity, unitPrice, lineTotal));
        }

        var subtotal = processedItems.Sum(item => item.LineTotal);
        var orderStatus = request.PaymentMethod == PaymentMethod.MockCard
            ? (request.MockCardSucceeded == true ? OrderStatus.Confirmed : OrderStatus.Cancelled)
            : OrderStatus.Confirmed;

        var paymentStatus = request.PaymentMethod switch
        {
            PaymentMethod.CashOnDelivery => PaymentStatus.Pending,
            PaymentMethod.MockCard when request.MockCardSucceeded == true => PaymentStatus.Succeeded,
            PaymentMethod.MockCard => PaymentStatus.Failed,
            _ => throw new InvalidOperationException("Unsupported payment method.")
        };

        var order = new Order
        {
            UserId = userId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            OrderStatus = orderStatus,
            CurrencyCode = "EGP",
            Subtotal = subtotal,
            FullName = request.Address.FullName,
            PhoneNumber = request.Address.PhoneNumber,
            Country = request.Address.Country,
            City = request.Address.City,
            StreetAddress = request.Address.StreetAddress,
            BuildingApartmentDetails = request.Address.BuildingApartmentDetails,
        };

        foreach (var item in processedItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.Product.Id,
                Product = item.Product,
                ProductName = item.Product.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.LineTotal,
            });

            if (orderStatus == OrderStatus.Confirmed)
            {
                item.Product.StockQuantity -= item.Quantity;
            }
        }

        order.Payment = new Payment
        {
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = paymentStatus,
            Amount = subtotal,
            CurrencyCode = "EGP",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var responseItems = order.Items
            .Select(item => new OrderItemSummary(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal))
            .ToList();

        return new CheckoutResult(
            order.Id,
            order.OrderStatus,
            order.Payment!.PaymentStatus,
            order.Subtotal,
            order.CurrencyCode,
            order.FullName,
            order.PhoneNumber,
            order.Country,
            order.City,
            order.StreetAddress,
            order.BuildingApartmentDetails,
            order.Payment.PaymentMethod,
            responseItems);
    }

    public async Task<OrderDetailsResponse?> GetOrderForUserAsync(Guid userId, int orderId)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Where(currentOrder => currentOrder.Id == orderId && currentOrder.UserId == userId)
            .Include(currentOrder => currentOrder.Payment)
            .Include(currentOrder => currentOrder.Items)
            .SingleOrDefaultAsync();

        return order is null ? null : ToDetailsResponse(order);
    }

    public async Task<IReadOnlyCollection<OrderDetailsResponse>> GetOrdersForUserAsync(Guid userId)
    {
        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(currentOrder => currentOrder.UserId == userId)
            .Include(currentOrder => currentOrder.Payment)
            .Include(currentOrder => currentOrder.Items)
            .OrderByDescending(currentOrder => currentOrder.Id)
            .ToListAsync();

        return orders.Select(ToDetailsResponse).ToList();
    }

    private static OrderDetailsResponse ToDetailsResponse(Order order)
    {
        var orderItems = order.Items
            .Select(item => new OrderItemSummary(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal))
            .ToList();

        return new OrderDetailsResponse(
            order.Id,
            order.OrderStatus.ToString(),
            order.Payment?.PaymentStatus.ToString() ?? PaymentStatus.Pending.ToString(),
            order.Subtotal,
            order.CurrencyCode,
            order.FullName,
            order.PhoneNumber,
            order.Country,
            order.City,
            order.StreetAddress,
            order.BuildingApartmentDetails,
            order.Payment?.PaymentMethod.ToString() ?? PaymentMethod.CashOnDelivery.ToString(),
            order.CreatedAtUtc,
            orderItems);
    }
}
