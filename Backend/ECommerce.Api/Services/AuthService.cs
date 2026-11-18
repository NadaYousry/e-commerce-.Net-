using System.ComponentModel.DataAnnotations;
using ECommerce.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Services;

public sealed record RegisterRequest
{
    [Required(ErrorMessage = "Display name is required.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 60 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(256, ErrorMessage = "Email must be 256 characters or fewer.")]
    [RegularExpression(ValidationRules.EmailPattern, ErrorMessage = "Enter a valid email address, e.g. name@example.com.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = ValidationRules.PasswordMinLength, ErrorMessage = "Password must be between 8 and 100 characters.")]
    public string Password { get; set; } = string.Empty;
}

public sealed record LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(ValidationRules.EmailPattern, ErrorMessage = "Enter a valid email address, e.g. name@example.com.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthResponse(string Token, string Email, string DisplayName, IReadOnlyCollection<string> Roles);

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}

public sealed class AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new RequestValidationException("Email", "An account with this email already exists. Try signing in instead.");
        }

        var user = new ApplicationUser
        {
            DisplayName = displayName,
            Email = email,
            UserName = email,
        };

        var creation = await userManager.CreateAsync(user, request.Password);
        if (!creation.Succeeded)
        {
            throw new RequestValidationException(IdentityErrorMapper.ToFieldErrors(creation.Errors));
        }

        return await CreateResponseAsync(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return null;
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return null;
        }

        return await CreateResponseAsync(user);
    }

    private async Task<AuthResponse> CreateResponseAsync(ApplicationUser user)
    {
        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var token = jwtTokenService.GenerateToken(user, roles);
        return new AuthResponse(token, user.Email!, user.DisplayName, roles);
    }
}
