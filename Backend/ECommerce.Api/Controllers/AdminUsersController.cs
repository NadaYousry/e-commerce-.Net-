using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ECommerce.Api.Entities;
using ECommerce.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers;

// "User" is not an Identity role: a regular user is simply one without the Admin role.
public static class UserRoleOptions
{
    public const string Admin = AppRoles.Admin;
    public const string User = "User";
    public const string Pattern = "^(Admin|User)$";
}

public sealed record AdminUserResponse(Guid Id, string DisplayName, string Email, string Role);

public sealed record CreateUserRequest
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

    [Required(ErrorMessage = "Role is required.")]
    [RegularExpression(UserRoleOptions.Pattern, ErrorMessage = "Role must be Admin or User.")]
    public string Role { get; set; } = UserRoleOptions.User;
}

public sealed record UpdateUserRoleRequest
{
    [Required(ErrorMessage = "Role is required.")]
    [RegularExpression(UserRoleOptions.Pattern, ErrorMessage = "Role must be Admin or User.")]
    public string Role { get; set; } = UserRoleOptions.User;
}

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminUsersController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminUserResponse>>> GetUsers()
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Email)
            .ToListAsync();

        var adminIds = (await userManager.GetUsersInRoleAsync(AppRoles.Admin))
            .Select(user => user.Id)
            .ToHashSet();

        return Ok(users.Select(user => ToResponse(user, adminIds.Contains(user.Id))).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserResponse>> GetUser(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(user, await userManager.IsInRoleAsync(user, AppRoles.Admin)));
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = request.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(nameof(CreateUserRequest.Email), "An account with this email already exists.");
            return ValidationProblem(ModelState);
        }

        var user = new ApplicationUser
        {
            DisplayName = request.DisplayName.Trim(),
            Email = email,
            UserName = email,
        };

        var creation = await userManager.CreateAsync(user, request.Password);
        if (!creation.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(IdentityErrorMapper.ToFieldErrors(creation.Errors)));
        }

        var isAdmin = request.Role == UserRoleOptions.Admin;
        if (isAdmin)
        {
            await EnsureAdminRoleExistsAsync();
            await userManager.AddToRoleAsync(user, AppRoles.Admin);
        }

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToResponse(user, isAdmin));
    }

    [HttpPut("{id:guid}/role")]
    public async Task<ActionResult<AdminUserResponse>> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var makeAdmin = request.Role == UserRoleOptions.Admin;
        var isAdmin = await userManager.IsInRoleAsync(user, AppRoles.Admin);

        // Stops an admin from locking themselves (and possibly everyone) out of the dashboard.
        if (!makeAdmin && isAdmin && IsCurrentUser(id))
        {
            return Problem(title: "You can't remove your own admin role.", statusCode: StatusCodes.Status409Conflict);
        }

        if (makeAdmin && !isAdmin)
        {
            await EnsureAdminRoleExistsAsync();
            await userManager.AddToRoleAsync(user, AppRoles.Admin);
        }
        else if (!makeAdmin && isAdmin)
        {
            await userManager.RemoveFromRoleAsync(user, AppRoles.Admin);
        }

        return Ok(ToResponse(user, makeAdmin));
    }

    private bool IsCurrentUser(Guid id) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId) && currentUserId == id;

    private async Task EnsureAdminRoleExistsAsync()
    {
        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(AppRoles.Admin));
        }
    }

    private static AdminUserResponse ToResponse(ApplicationUser user, bool isAdmin) => new(
        user.Id,
        user.DisplayName,
        user.Email ?? string.Empty,
        isAdmin ? UserRoleOptions.Admin : UserRoleOptions.User);
}
