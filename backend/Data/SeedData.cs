using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceApi.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ECommerceDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ECommerceDbContext>>());

            if (await context.Users.AnyAsync())
            {
                return;
            }

            var admin = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                PasswordHash = HashPassword("Admin123!"),
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var testUser = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = "user@example.com",
                PasswordHash = HashPassword("User123!"),
                Phone = "+0987654321",
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var users = new[]
            {
                admin,
                testUser,
                new User
                {
                    FirstName = "Anna",
                    LastName = "Ivanova",
                    Email = "anna@example.com",
                    PasswordHash = HashPassword("password123"),
                    Phone = "+7 (999) 123-45-67",
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    FirstName = "Peter",
                    LastName = "Petrov",
                    Email = "peter@example.com",
                    PasswordHash = HashPassword("password123"),
                    Phone = "+7 (999) 765-43-21",
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            var products = new[]
            {
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Description = "Latest Apple smartphone with A17 Pro chip",
                    Price = 89999.99m,
                    StockQuantity = 50,
                    Category = "Electronics",
                    Brand = "Apple",
                    ImageUrl = "https://example.com/iphone15.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Samsung Galaxy S24",
                    Description = "Flagship Android smartphone with 200MP camera",
                    Price = 74999.99m,
                    StockQuantity = 30,
                    Category = "Electronics",
                    Brand = "Samsung",
                    ImageUrl = "https://example.com/galaxy-s24.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "AirPods Pro 2",
                    Description = "Wireless earbuds with active noise cancellation",
                    Price = 24999.99m,
                    StockQuantity = 100,
                    Category = "Electronics",
                    Brand = "Apple",
                    ImageUrl = "https://example.com/airpods-pro.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "MacBook Air M3",
                    Description = "Ultra-thin laptop with M3 chip and Retina display",
                    Price = 129999.99m,
                    StockQuantity = 25,
                    Category = "Electronics",
                    Brand = "Apple",
                    ImageUrl = "https://example.com/macbook-air.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new Product
                {
                    Name = "Winter Jacket",
                    Description = "Warm winter jacket with water-repellent coating",
                    Price = 8999.99m,
                    StockQuantity = 40,
                    Category = "Clothing",
                    Brand = "Columbia",
                    ImageUrl = "https://example.com/winter-jacket.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Classic Jeans",
                    Description = "Classic jeans made from quality denim",
                    Price = 4999.99m,
                    StockQuantity = 60,
                    Category = "Clothing",
                    Brand = "Levi's",
                    ImageUrl = "https://example.com/jeans.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new Product
                {
                    Name = "DeLonghi Coffee Machine",
                    Description = "Automatic coffee machine with built-in grinder",
                    Price = 45999.99m,
                    StockQuantity = 15,
                    Category = "Home & Garden",
                    Brand = "DeLonghi",
                    ImageUrl = "https://example.com/coffee-machine.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new Product
                {
                    Name = "Running Shoes",
                    Description = "Professional running shoes with cushioning",
                    Price = 12999.99m,
                    StockQuantity = 80,
                    Category = "Sports",
                    Brand = "Nike",
                    ImageUrl = "https://example.com/running-shoes.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new Product
                {
                    Name = "C# Programming Guide",
                    Description = "Complete guide to C# and .NET development",
                    Price = 2999.99m,
                    StockQuantity = 200,
                    Category = "Books",
                    Brand = "O'Reilly",
                    ImageUrl = "https://example.com/csharp-book.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new Product
                {
                    Name = "React: Modern Development",
                    Description = "Learning React and modern approaches to frontend development",
                    Price = 2599.99m,
                    StockQuantity = 150,
                    Category = "Books",
                    Brand = "O'Reilly",
                    ImageUrl = "https://example.com/react-book.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            var cartItems = new[]
            {
                new CartItem
                {
                    UserId = 2,
                    ProductId = 1,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new CartItem
                {
                    UserId = 2,
                    ProductId = 3,
                    Quantity = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.CartItems.AddRange(cartItems);
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}