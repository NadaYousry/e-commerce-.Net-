using ECommerce.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers;

public sealed record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string CurrencyCode,
    int StockQuantity,
    string ImageUrl,
    bool IsActive);

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(ECommerceDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderBy(product => product.Id)
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.CurrencyCode,
                product.StockQuantity,
                product.ImageUrl,
                product.IsActive))
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id && product.IsActive)
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.CurrencyCode,
                product.StockQuantity,
                product.ImageUrl,
                product.IsActive))
            .SingleOrDefaultAsync();

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
