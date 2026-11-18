namespace ECommerce.Api.Entities;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
    public int StockQuantity { get; set; }
    public required string ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();
}