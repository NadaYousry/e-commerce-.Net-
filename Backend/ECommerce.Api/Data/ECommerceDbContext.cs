using ECommerce.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Data;

public sealed class ECommerceDbContext(
    DbContextOptions<ECommerceDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("EmailIndex");
            entity.HasMany(user => user.Orders)
                .WithOne(order => order.User)
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name).HasMaxLength(200).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(2000).IsRequired();
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.Property(product => product.CurrencyCode).HasMaxLength(3).HasDefaultValue("EGP").IsRequired();
            entity.Property(product => product.ImageUrl).HasMaxLength(2048).IsRequired();
            entity.HasIndex(product => product.Name);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(order => order.OrderStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(order => order.CurrencyCode).HasMaxLength(3).HasDefaultValue("EGP").IsRequired();
            entity.Property(order => order.Subtotal).HasPrecision(18, 2);
            entity.Property(order => order.FullName).HasMaxLength(160).IsRequired();
            entity.Property(order => order.PhoneNumber).HasMaxLength(40).IsRequired();
            entity.Property(order => order.Country).HasMaxLength(100).IsRequired();
            entity.Property(order => order.City).HasMaxLength(100).IsRequired();
            entity.Property(order => order.StreetAddress).HasMaxLength(300).IsRequired();
            entity.Property(order => order.BuildingApartmentDetails).HasMaxLength(200);
            entity.HasIndex(order => new { order.UserId, order.CreatedAtUtc });
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Property(item => item.LineTotal).HasPrecision(18, 2);
            entity.HasOne(item => item.Product)
                .WithMany(product => product.OrderItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.Property(payment => payment.PaymentMethod).HasConversion<string>().HasMaxLength(32);
            entity.Property(payment => payment.PaymentStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.Property(payment => payment.CurrencyCode).HasMaxLength(3).HasDefaultValue("EGP").IsRequired();
            entity.HasIndex(payment => payment.OrderId).IsUnique();
            entity.HasOne(payment => payment.Order)
                .WithOne(order => order.Payment)
                .HasForeignKey<Payment>(payment => payment.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}