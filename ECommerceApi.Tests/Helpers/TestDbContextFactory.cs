using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;

namespace ECommerceApi.Tests.Helpers
{
    public class TestDbContextFactory
    {
        public static ECommerceDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Уникальная БД для каждого теста
                .Options;

            var context = new ECommerceDbContext(options);
            return context;
        }

        public static async Task<ECommerceDbContext> CreateContextWithTestData()
        {
            var context = CreateInMemoryContext();
            
            // Добавляем тестовые данные
            var users = new[]
            {
                new User 
                { 
                    Id = 1, 
                    FirstName = "Test", 
                    LastName = "User", 
                    Email = "test@example.com", 
                    PasswordHash = "hashedpassword",
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Id = 2, 
                    FirstName = "Admin", 
                    LastName = "User", 
                    Email = "admin@example.com", 
                    PasswordHash = "hashedpassword",
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Test iPhone",
                    Description = "Test iPhone description",
                    Price = 50000m,
                    StockQuantity = 10,
                    Category = "Electronics",
                    Brand = "Apple",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Name = "Test Samsung",
                    Description = "Test Samsung description", 
                    Price = 40000m,
                    StockQuantity = 5,
                    Category = "Electronics",
                    Brand = "Samsung",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 3,
                    Name = "Test Inactive Product",
                    Description = "This product is inactive",
                    Price = 10000m,
                    StockQuantity = 0,
                    Category = "Electronics", 
                    Brand = "TestBrand",
                    IsActive = false, // ← Неактивный товар
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Users.AddRange(users);
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            return context;
        }
    }
}