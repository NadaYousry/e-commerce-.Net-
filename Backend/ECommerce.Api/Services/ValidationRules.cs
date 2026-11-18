namespace ECommerce.Api.Services;

/// <summary>
/// Shared validation patterns. Keep in sync with frontend/src/validation.js.
/// </summary>
public static class ValidationRules
{
    public const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
    public const string PhonePattern = @"^\+?\d[\d\s-]{6,19}$";
    public const int PasswordMinLength = 8;
}
