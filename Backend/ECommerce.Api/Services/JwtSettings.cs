namespace ECommerce.Api.Services;

public sealed class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "ECommerce.Api";
    public string Audience { get; set; } = "ECommerce.Api.Users";
    public int ExpiryHours { get; set; } = 1;
}
