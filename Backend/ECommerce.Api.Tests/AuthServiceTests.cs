using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using ECommerce.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Tests;

public sealed class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public AuthServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserAndReturnsToken()
    {
        await using var context = CreateContext();
        var userManager = CreateUserManager(context);
        var tokenService = new JwtTokenService(Options.Create(new JwtSettings
        {
            Key = "this-is-a-very-secure-key-1234567890",
            Issuer = "ECommerce.Api",
            Audience = "ECommerce.Api.Users",
            ExpiryHours = 1
        }));
        var service = new AuthService(userManager, tokenService);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Alice",
            Email = "alice@example.com",
            Password = "Password123!"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("alice@example.com", result.Email);
        Assert.Equal("Alice", result.DisplayName);
        Assert.True(await context.Users.AnyAsync(user => user.Email == "alice@example.com"));
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_ForValidCredentials()
    {
        await using var context = CreateContext();
        var userManager = CreateUserManager(context);
        var tokenService = new JwtTokenService(Options.Create(new JwtSettings
        {
            Key = "this-is-a-very-secure-key-1234567890",
            Issuer = "ECommerce.Api",
            Audience = "ECommerce.Api.Users",
            ExpiryHours = 1
        }));

        var user = new ApplicationUser
        {
            DisplayName = "Bob",
            Email = "bob@example.com",
            UserName = "bob@example.com"
        };
        var password = "Password123!";
        var createResult = await userManager.CreateAsync(user, password);
        Assert.True(createResult.Succeeded);

        var service = new AuthService(userManager, tokenService);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "bob@example.com",
            Password = password
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("bob@example.com", result.Email);
        Assert.Equal("Bob", result.DisplayName);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsEmailError_WhenEmailAlreadyExists()
    {
        await using var context = CreateContext();
        var service = new AuthService(CreateUserManager(context), CreateTokenService());

        await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Carol",
            Email = "carol@example.com",
            Password = "Password123!"
        });

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Carol Again",
            Email = "carol@example.com",
            Password = "Password123!"
        }));

        Assert.True(exception.Errors.ContainsKey("Email"));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsPasswordErrors_WhenPasswordIsWeak()
    {
        await using var context = CreateContext();
        var service = new AuthService(CreateUserManager(context), CreateTokenService());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Dave",
            Email = "dave@example.com",
            Password = "password"
        }));

        Assert.True(exception.Errors.ContainsKey("Password"));
        Assert.NotEmpty(exception.Errors["Password"]);
        Assert.False(await context.Users.AnyAsync(user => user.Email == "dave@example.com"));
    }

    [Fact]
    public async Task LoginAsync_ReturnsAdminRole_ForAdminUser()
    {
        await using var context = CreateContext();
        var userManager = CreateUserManager(context);
        context.Roles.Add(new IdentityRole<Guid>(AppRoles.Admin) { NormalizedName = AppRoles.Admin.ToUpperInvariant() });
        await context.SaveChangesAsync();

        var admin = new ApplicationUser
        {
            DisplayName = "Admin",
            Email = "admin@example.com",
            UserName = "admin@example.com"
        };
        Assert.True((await userManager.CreateAsync(admin, "Password123!")).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(admin, AppRoles.Admin)).Succeeded);

        var service = new AuthService(userManager, CreateTokenService());

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@example.com",
            Password = "Password123!"
        });

        Assert.NotNull(result);
        Assert.Contains(AppRoles.Admin, result.Roles);
    }

    private static JwtTokenService CreateTokenService()
    {
        return new JwtTokenService(Options.Create(new JwtSettings
        {
            Key = "this-is-a-very-secure-key-1234567890",
            Issuer = "ECommerce.Api",
            Audience = "ECommerce.Api.Users",
            ExpiryHours = 1
        }));
    }

    private ECommerceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new ECommerceDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static UserManager<ApplicationUser> CreateUserManager(ECommerceDbContext context)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ECommerceDbContext>(options => options.UseSqlite(context.Database.GetDbConnection()));
        services.AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ECommerceDbContext>();

        return services.BuildServiceProvider().GetRequiredService<UserManager<ApplicationUser>>();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
