using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Seeding;

public static class DevelopmentProductSeeder
{
    public static async Task SeedAsync(ECommerceDbContext database)
    {
        if (await database.Products.AnyAsync())
        {
            return;
        }

        database.Products.AddRange(
            new Product
            {
                Name = "Canvas Weekender Bag",
                Description = "A durable everyday bag with a roomy main compartment.",
                Price = 1850m,
                StockQuantity = 18,
                ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=900",
            },
            new Product
            {
                Name = "Ceramic Pour-Over Set",
                Description = "A glazed ceramic dripper and matching cup for slow mornings.",
                Price = 1325m,
                StockQuantity = 12,
                ImageUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=900",
            },
            new Product
            {
                Name = "Desk Light",
                Description = "A compact adjustable lamp for focused evening work.",
                Price = 2475m,
                StockQuantity = 9,
                ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=900",
            });

        await database.SaveChangesAsync();
    }
}