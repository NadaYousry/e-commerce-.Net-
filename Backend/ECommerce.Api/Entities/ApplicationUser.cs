using Microsoft.AspNetCore.Identity;

namespace ECommerce.Api.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public required string DisplayName { get; set; }
    public ICollection<Order> Orders { get; } = new List<Order>();
}