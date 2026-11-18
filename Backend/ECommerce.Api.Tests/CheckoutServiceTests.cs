using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using ECommerce.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Tests;

public sealed class CheckoutServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public CheckoutServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    [Fact]
    public async Task CheckoutAsync_WhenMockCardSucceeds_ConfirmsOrderAndReducesStock()
    {
        await using var context = CreateContext();
        var user = new ApplicationUser
        {
            DisplayName = "Checkout User",
            Email = "checkout@example.com",
            UserName = "checkout@example.com"
        };
        var product = new Product
        {
            Name = "Demo Product",
            Description = "For checkout tests",
            Price = 200m,
            CurrencyCode = "EGP",
            StockQuantity = 5,
            ImageUrl = "https://example.com/demo.jpg",
            IsActive = true
        };
        context.Users.Add(user);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new CheckoutService(context);
        var request = new CheckoutRequest(
            new[] { new CheckoutLineRequest(product.Id, 2) },
            new CheckoutAddressRequest("Checkout User", "+966500000000", "Saudi Arabia", "Riyadh", "Main Street", "Apartment 7"),
            PaymentMethod.MockCard,
            true);

        var result = await service.CheckoutAsync(user.Id, request);

        Assert.Equal(OrderStatus.Confirmed, result.OrderStatus);
        Assert.Equal(PaymentStatus.Succeeded, result.PaymentStatus);
        Assert.Equal(400m, result.Subtotal);
        Assert.Equal(3, await context.Products.Where(p => p.Id == product.Id).Select(p => p.StockQuantity).SingleAsync());
    }

    [Fact]
    public async Task CheckoutAsync_WhenMockCardDeclines_CancelsOrderAndKeepsStock()
    {
        await using var context = CreateContext();
        var user = new ApplicationUser
        {
            DisplayName = "Checkout User",
            Email = "checkout-decline@example.com",
            UserName = "checkout-decline@example.com"
        };
        var product = new Product
        {
            Name = "Declined Product",
            Description = "For decline tests",
            Price = 150m,
            CurrencyCode = "EGP",
            StockQuantity = 4,
            ImageUrl = "https://example.com/decline.jpg",
            IsActive = true
        };
        context.Users.Add(user);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new CheckoutService(context);
        var request = new CheckoutRequest(
            new[] { new CheckoutLineRequest(product.Id, 1) },
            new CheckoutAddressRequest("Checkout User", "+966500000001", "Saudi Arabia", "Jeddah", "Ali Road", null),
            PaymentMethod.MockCard,
            false);

        var result = await service.CheckoutAsync(user.Id, request);

        Assert.Equal(OrderStatus.Cancelled, result.OrderStatus);
        Assert.Equal(PaymentStatus.Failed, result.PaymentStatus);
        Assert.Equal(150m, result.Subtotal);
        Assert.Equal(4, await context.Products.Where(p => p.Id == product.Id).Select(p => p.StockQuantity).SingleAsync());
    }

    private ECommerceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new ECommerceDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
