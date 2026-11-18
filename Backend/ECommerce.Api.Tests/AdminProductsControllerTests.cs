using ECommerce.Api.Controllers;
using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Tests;

public sealed class AdminProductsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public AdminProductsControllerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    [Fact]
    public async Task GetProducts_IncludesInactiveProducts()
    {
        await using var context = CreateContext();
        context.Products.AddRange(CreateProduct("Active", isActive: true), CreateProduct("Hidden", isActive: false));
        await context.SaveChangesAsync();

        var result = await new AdminProductsController(context).GetProducts();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task CreateThenUpdateProduct_PersistsChanges()
    {
        await using var context = CreateContext();
        var controller = new AdminProductsController(context);

        var created = await controller.CreateProduct(new ProductUpsertRequest(
            "  Linen Shirt  ", "A breathable linen shirt for warm days.", 950m, 20, "https://example.com/shirt.jpg"));

        var createdResult = Assert.IsType<CreatedAtActionResult>(created.Result);
        var createdProduct = Assert.IsType<ProductResponse>(createdResult.Value);
        Assert.Equal("Linen Shirt", createdProduct.Name);

        var updated = await controller.UpdateProduct(createdProduct.Id, new ProductUpsertRequest(
            "Linen Shirt", "A breathable linen shirt for warm days.", 800m, 5, "https://example.com/shirt.jpg", IsActive: false));

        Assert.IsType<OkObjectResult>(updated.Result);
        var stored = await context.Products.AsNoTracking().SingleAsync(product => product.Id == createdProduct.Id);
        Assert.Equal(800m, stored.Price);
        Assert.Equal(5, stored.StockQuantity);
        Assert.False(stored.IsActive);
    }

    [Fact]
    public async Task DeleteProduct_RemovesProductWithoutOrders()
    {
        await using var context = CreateContext();
        var product = CreateProduct("Disposable", isActive: true);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var result = await new AdminProductsController(context).DeleteProduct(product.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.False(await context.Products.AnyAsync());
    }

    [Fact]
    public async Task UpdateAndDelete_ReturnNotFound_ForMissingProduct()
    {
        await using var context = CreateContext();
        var controller = new AdminProductsController(context);

        var update = await controller.UpdateProduct(999, new ProductUpsertRequest(
            "Missing", "This product does not exist.", 10m, 1, "https://example.com/missing.jpg"));
        var delete = await controller.DeleteProduct(999);

        Assert.IsType<NotFoundResult>(update.Result);
        Assert.IsType<NotFoundResult>(delete);
    }

    private static Product CreateProduct(string name, bool isActive) => new()
    {
        Name = name,
        Description = $"{name} description",
        Price = 100m,
        StockQuantity = 3,
        ImageUrl = "https://example.com/product.jpg",
        IsActive = isActive,
    };

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
