using System.Security.Claims;
using ECommerce.Api.Controllers;
using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Api.Tests;

public sealed class AdminUsersControllerTests : IDisposable
{
    private const string Password = "Password123!";
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _services;

    public AdminUsersControllerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ECommerceDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ECommerceDbContext>();
        _services = services.BuildServiceProvider();

        _services.GetRequiredService<ECommerceDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateUser_WithAdminRole_CreatesAdmin()
    {
        var controller = CreateController(currentUserId: Guid.NewGuid());

        var result = await controller.CreateUser(new CreateUserRequest
        {
            DisplayName = "  New Admin  ",
            Email = "new.admin@example.com",
            Password = Password,
            Role = UserRoleOptions.Admin,
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<AdminUserResponse>(created.Value);
        Assert.Equal("New Admin", response.DisplayName);
        Assert.Equal(UserRoleOptions.Admin, response.Role);

        var stored = await UserManager.FindByEmailAsync("new.admin@example.com");
        Assert.NotNull(stored);
        Assert.True(await UserManager.IsInRoleAsync(stored, AppRoles.Admin));
    }

    [Fact]
    public async Task CreateUser_ReturnsValidationProblem_ForDuplicateEmail()
    {
        await CreateUserAsync("taken@example.com", isAdmin: false);
        var controller = CreateController(currentUserId: Guid.NewGuid());

        var result = await controller.CreateUser(new CreateUserRequest
        {
            DisplayName = "Someone",
            Email = "taken@example.com",
            Password = Password,
            Role = UserRoleOptions.User,
        });

        var problem = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.True(details.Errors.ContainsKey(nameof(CreateUserRequest.Email)));
    }

    [Fact]
    public async Task GetUsers_ReturnsRoleForEachUser()
    {
        await CreateUserAsync("admin@example.com", isAdmin: true);
        await CreateUserAsync("shopper@example.com", isAdmin: false);

        var result = await CreateController(currentUserId: Guid.NewGuid()).GetUsers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<AdminUserResponse>>(ok.Value).ToList();
        Assert.Equal(UserRoleOptions.Admin, users.Single(user => user.Email == "admin@example.com").Role);
        Assert.Equal(UserRoleOptions.User, users.Single(user => user.Email == "shopper@example.com").Role);
    }

    [Fact]
    public async Task UpdateRole_PromotesAndDemotesUser()
    {
        var user = await CreateUserAsync("shopper@example.com", isAdmin: false);
        var controller = CreateController(currentUserId: Guid.NewGuid());

        var promoted = await controller.UpdateRole(user.Id, new UpdateUserRoleRequest { Role = UserRoleOptions.Admin });
        Assert.Equal(UserRoleOptions.Admin, Assert.IsType<AdminUserResponse>(Assert.IsType<OkObjectResult>(promoted.Result).Value).Role);
        Assert.True(await UserManager.IsInRoleAsync(user, AppRoles.Admin));

        var demoted = await controller.UpdateRole(user.Id, new UpdateUserRoleRequest { Role = UserRoleOptions.User });
        Assert.Equal(UserRoleOptions.User, Assert.IsType<AdminUserResponse>(Assert.IsType<OkObjectResult>(demoted.Result).Value).Role);
        Assert.False(await UserManager.IsInRoleAsync(user, AppRoles.Admin));
    }

    [Fact]
    public async Task UpdateRole_RejectsRemovingOwnAdminRole()
    {
        var admin = await CreateUserAsync("admin@example.com", isAdmin: true);
        var controller = CreateController(currentUserId: admin.Id);

        var result = await controller.UpdateRole(admin.Id, new UpdateUserRoleRequest { Role = UserRoleOptions.User });

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.True(await UserManager.IsInRoleAsync(admin, AppRoles.Admin));
    }

    [Fact]
    public async Task UpdateRole_ReturnsNotFound_ForMissingUser()
    {
        var result = await CreateController(currentUserId: Guid.NewGuid())
            .UpdateRole(Guid.NewGuid(), new UpdateUserRoleRequest { Role = UserRoleOptions.Admin });

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private UserManager<ApplicationUser> UserManager => _services.GetRequiredService<UserManager<ApplicationUser>>();

    private RoleManager<IdentityRole<Guid>> RoleManager => _services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    private async Task<ApplicationUser> CreateUserAsync(string email, bool isAdmin)
    {
        var user = new ApplicationUser { DisplayName = email.Split('@')[0], Email = email, UserName = email };
        Assert.True((await UserManager.CreateAsync(user, Password)).Succeeded);

        if (isAdmin)
        {
            if (!await RoleManager.RoleExistsAsync(AppRoles.Admin))
            {
                await RoleManager.CreateAsync(new IdentityRole<Guid>(AppRoles.Admin));
            }
            Assert.True((await UserManager.AddToRoleAsync(user, AppRoles.Admin)).Succeeded);
        }

        return user;
    }

    private AdminUsersController CreateController(Guid currentUserId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()), new Claim(ClaimTypes.Role, AppRoles.Admin)],
            "Test");

        return new AdminUsersController(UserManager, RoleManager)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
            },
        };
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }
}
