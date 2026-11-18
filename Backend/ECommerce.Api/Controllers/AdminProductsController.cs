using System.ComponentModel.DataAnnotations;
using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers;

public sealed record ProductUpsertRequest(
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
    string Name,
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
    string Description,
    [Range(0.01, 1_000_000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
    decimal Price,
    [Range(0, 100_000, ErrorMessage = "Stock must be a whole number between 0 and 100,000.")]
    int StockQuantity,
    [Required(ErrorMessage = "Image URL is required.")]
    [StringLength(2048, ErrorMessage = "Image URL must be 2048 characters or fewer.")]
    [Url(ErrorMessage = "Enter a full image URL starting with http:// or https://.")]
    string ImageUrl,
    bool IsActive = true);

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminProductsController(ECommerceDbContext dbContext) : ControllerBase
{
    // Unlike the public endpoint, admins also see inactive products.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .ToListAsync();

        return Ok(products.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id)
    {
        var product = await dbContext.Products.AsNoTracking().SingleOrDefaultAsync(product => product.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] ProductUpsertRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl.Trim(),
            IsActive = request.IsActive,
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ToResponse(product));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(int id, [FromBody] ProductUpsertRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var product = await dbContext.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl.Trim();
        product.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(product));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await dbContext.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        // Order history references products, so ordered products can only be hidden, not removed.
        if (await dbContext.OrderItems.AnyAsync(item => item.ProductId == id))
        {
            return Problem(
                title: $"'{product.Name}' appears in existing orders and can't be deleted. Mark it as inactive to hide it from the store instead.",
                statusCode: StatusCodes.Status409Conflict);
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static ProductResponse ToResponse(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.CurrencyCode,
        product.StockQuantity,
        product.ImageUrl,
        product.IsActive);
}
