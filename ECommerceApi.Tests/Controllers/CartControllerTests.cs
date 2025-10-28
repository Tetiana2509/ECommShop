using Xunit;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Controllers;
using ECommerceApi.Models;
using ECommerceApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests.Controllers
{
    public class CartControllerTests : IDisposable
    {
        private readonly CartController _controller;
        private readonly ECommerceApi.Data.ECommerceDbContext _context;

        public CartControllerTests()
        {
            _context = TestDbContextFactory.CreateContextWithTestData().Result;
            _controller = new CartController(_context);
        }

        [Fact]
        public async Task GetCart_WithEmptyCart_ReturnsEmptyCart()
        {
            // Act - получаем корзину пользователя без товаров
            var result = await _controller.GetCart(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cartSummary = okResult.Value;
            
            var itemsProperty = cartSummary.GetType().GetProperty("Items");
            var items = (IEnumerable<object>)itemsProperty.GetValue(cartSummary);
            
            var totalItemsProperty = cartSummary.GetType().GetProperty("TotalItems");
            var totalItems = (int)totalItemsProperty.GetValue(cartSummary);
            
            var totalAmountProperty = cartSummary.GetType().GetProperty("TotalAmount");
            var totalAmount = (decimal)totalAmountProperty.GetValue(cartSummary);
            
            Assert.Empty(items);
            Assert.Equal(0, totalItems);
            Assert.Equal(0m, totalAmount);
        }

        [Fact]
        public async Task AddToCart_WithValidProduct_AddsToCart()
        {
            // Arrange
            var request = new AddToCartRequest
            {
                UserId = 1,
                ProductId = 1,
                Quantity = 2
            };

            // Act
            var result = await _controller.AddToCart(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            
            // Проверяем что товар добавился в БД
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == 1 && c.ProductId == 1);
            
            Assert.NotNull(cartItem);
            Assert.Equal(2, cartItem.Quantity);
        }

        [Fact]
        public async Task AddToCart_WithNonExistentProduct_ReturnsNotFound()
        {
            // Arrange
            var request = new AddToCartRequest
            {
                UserId = 1,
                ProductId = 999, // Несуществующий товар
                Quantity = 1
            };

            // Act
            var result = await _controller.AddToCart(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Product not found", notFoundResult.Value);
        }

        [Fact]
        public async Task AddToCart_WithInactiveProduct_ReturnsNotFound()
        {
            // Arrange
            var request = new AddToCartRequest
            {
                UserId = 1,
                ProductId = 3, // Неактивный товар
                Quantity = 1
            };

            // Act
            var result = await _controller.AddToCart(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Product not found", notFoundResult.Value);
        }

        [Fact]
        public async Task AddToCart_WithInsufficientStock_ReturnsBadRequest()
        {
            // Arrange
            var request = new AddToCartRequest
            {
                UserId = 1,
                ProductId = 2, // У товара StockQuantity = 5
                Quantity = 10  // Запрашиваем больше чем есть
            };

            // Act
            var result = await _controller.AddToCart(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Not enough stock", badRequestResult.Value);
        }

        [Fact]
        public async Task AddToCart_WithExistingItem_UpdatesQuantity()
        {
            // Arrange - добавляем товар в корзину первый раз
            var initialItem = new CartItem
            {
                UserId = 1,
                ProductId = 1,
                Quantity = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(initialItem);
            await _context.SaveChangesAsync();

            var request = new AddToCartRequest
            {
                UserId = 1,
                ProductId = 1, // Тот же товар
                Quantity = 2   // Добавляем еще 2
            };

            // Act
            var result = await _controller.AddToCart(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == 1 && c.ProductId == 1);
            
            Assert.NotNull(cartItem);
            Assert.Equal(3, cartItem.Quantity); // 1 + 2 = 3
        }

        [Fact]
        public async Task GetCart_WithItems_ReturnsCartWithCorrectTotals()
        {
            // Arrange - добавляем товары в корзину
            var cartItems = new[]
            {
                new CartItem { UserId = 1, ProductId = 1, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, // 2 × 50000 = 100000
                new CartItem { UserId = 1, ProductId = 2, Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }  // 1 × 40000 = 40000
            };
            _context.CartItems.AddRange(cartItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCart(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cartSummary = okResult.Value;
            
            var itemsProperty = cartSummary.GetType().GetProperty("Items");
            var items = (IEnumerable<object>)itemsProperty.GetValue(cartSummary);
            
            var totalItemsProperty = cartSummary.GetType().GetProperty("TotalItems");
            var totalItems = (int)totalItemsProperty.GetValue(cartSummary);
            
            var totalAmountProperty = cartSummary.GetType().GetProperty("TotalAmount");
            var totalAmount = (decimal)totalAmountProperty.GetValue(cartSummary);
            
            Assert.Equal(2, items.Count()); // 2 разных товара
            Assert.Equal(3, totalItems);    // Общее количество: 2 + 1 = 3
            Assert.Equal(140000m, totalAmount); // Общая сумма: 100000 + 40000 = 140000
        }

        [Fact]
        public async Task UpdateCartItem_WithValidData_UpdatesQuantity()
        {
            // Arrange
            var cartItem = new CartItem
            {
                UserId = 1,
                ProductId = 1,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateCartRequest { Quantity = 5 };

            // Act
            var result = await _controller.UpdateCartItem(cartItem.Id, updateRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            var updatedItem = await _context.CartItems.FindAsync(cartItem.Id);
            Assert.Equal(5, updatedItem.Quantity);
        }

        [Fact]
        public async Task UpdateCartItem_WithInsufficientStock_ReturnsBadRequest()
        {
            // Arrange
            var cartItem = new CartItem
            {
                UserId = 1,
                ProductId = 2, // StockQuantity = 5
                Quantity = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateCartRequest { Quantity = 10 }; // Больше чем на складе

            // Act
            var result = await _controller.UpdateCartItem(cartItem.Id, updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Not enough stock", badRequestResult.Value);
        }

        [Fact]
        public async Task RemoveFromCart_WithValidId_RemovesItem()
        {
            // Arrange
            var cartItem = new CartItem
            {
                UserId = 1,
                ProductId = 1,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.RemoveFromCart(cartItem.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            var removedItem = await _context.CartItems.FindAsync(cartItem.Id);
            Assert.Null(removedItem); // Товар удален из БД
        }

        [Fact]
        public async Task RemoveFromCart_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.RemoveFromCart(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ClearCart_RemovesAllUserItems()
        {
            // Arrange - добавляем товары в корзину для разных пользователей
            var cartItems = new[]
            {
                new CartItem { UserId = 1, ProductId = 1, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new CartItem { UserId = 1, ProductId = 2, Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new CartItem { UserId = 2, ProductId = 1, Quantity = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow } // Другой пользователь
            };
            _context.CartItems.AddRange(cartItems);
            await _context.SaveChangesAsync();

            // Act - очищаем корзину пользователя 1
            var result = await _controller.ClearCart(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            var remainingItems = await _context.CartItems.Where(c => c.UserId == 1).ToListAsync();
            Assert.Empty(remainingItems); // Корзина пользователя 1 очищена
            
            var otherUserItems = await _context.CartItems.Where(c => c.UserId == 2).ToListAsync();
            Assert.Single(otherUserItems); // Корзина пользователя 2 не тронута
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}