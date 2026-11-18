namespace ECommerce.Api.Entities;

public enum OrderStatus
{
    Confirmed,
    Cancelled
}

public sealed class Order
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
    public decimal Subtotal { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string StreetAddress { get; set; }
    public string? BuildingApartmentDetails { get; set; }
    public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
}