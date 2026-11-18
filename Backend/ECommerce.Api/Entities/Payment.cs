namespace ECommerce.Api.Entities;

public enum PaymentMethod
{
    CashOnDelivery,
    MockCard
}

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed
}

public sealed class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
    public DateTimeOffset CreatedAtUtc { get; set; }
}