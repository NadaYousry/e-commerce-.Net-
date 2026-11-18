using ECommerce.Api.Controllers;
using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Tests;

public sealed class ProductsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public ProductsControllerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    [Fact]
    public async Task GetProducts_ReturnsOnlyActiveProducts()
    {
        await using var context = CreateContext();
        context.Products.AddRange(
            new Product
            {
                Name = "Active Product",
                Description = "Visible to customers",
                Price = 150m,
                CurrencyCode = "EGP",
                StockQuantity = 10,
                ImageUrl = "https://example.com/active.jpg",
                IsActive = true
            },
            new Product
            {
                Name = "Hidden Product",
                Description = "Not visible to customers",
                Price = 200m,
                CurrencyCode = "EGP",
                StockQuantity = 5,
                ImageUrl = "https://example.com/hidden.jpg",
                IsActive = false
            });
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);

        var result = await controller.GetProducts();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        var productList = products.ToList();

        Assert.Single(productList);
        Assert.Equal("Active Product", productList[0].Name);
    }

    [Fact]
    public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExistOrIsInactive()
    {
        await using var context = CreateContext();
        context.Products.Add(new Product
        {
            Name = "Inactive Product",
            Description = "Not visible",
            Price = 100m,
            CurrencyCode = "EGP",
            StockQuantity = 3,
            ImageUrl = "https://example.com/inactive.jpg",
            IsActive = false
        });
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);

        var missingResult = await controller.GetProduct(9999);
        var inactiveResult = await controller.GetProduct(1);

        Assert.IsType<NotFoundResult>(missingResult.Result);
        Assert.IsType<NotFoundResult>(inactiveResult.Result);
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
