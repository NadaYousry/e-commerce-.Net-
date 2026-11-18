using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Services;

public static class IdentityErrorMapper
{
    // Identity error codes are prefixed by the field they relate to (PasswordTooShort, DuplicateEmail, ...).
    public static Dictionary<string, string[]> ToFieldErrors(IEnumerable<IdentityError> errors)
    {
        return errors
            .GroupBy(error => error.Code switch
            {
                var code when code.StartsWith("Password", StringComparison.Ordinal) => "Password",
                var code when code.Contains("Email", StringComparison.Ordinal) || code.Contains("UserName", StringComparison.Ordinal) => "Email",
                _ => string.Empty,
            })
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
    }
}
