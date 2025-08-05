using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Models;

namespace Order.Infrastructure.Data
{
    public static class OrderSeedDbContext
    {
        public static async Task SeedAsync(OrderDbContext context)
        {
          
            // Products Seeding
            if (!await context.Products.AnyAsync())
            {
                var productData = await File.ReadAllTextAsync("../Order.Infrastructure/Data/DataSeed/products.json");
                var products = JsonSerializer.Deserialize<List<Product>>(productData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (products?.Count > 0) 
                {
                    foreach (var product in products)
                    {
                        await context.Set<Product>().AddAsync(product);
                    }
                    await context.SaveChangesAsync();
                }
            }

            
        }
    }
}
