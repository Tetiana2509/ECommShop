using Xunit;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Controllers;
using ECommerceApi.Models;
using ECommerceApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests.Controllers
{
    public class ProductsControllerTests : IDisposable
    {
        private readonly ProductsController _controller;
        private readonly ECommerceApi.Data.ECommerceDbContext _context;

        public ProductsControllerTests()
        {
            _context = TestDbContextFactory.CreateContextWithTestData().Result;
            _controller = new ProductsController(_context);
        }

        [Fact]
        public async Task GetProducts_ReturnsActiveProducts()
        {
            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = okResult.Value;
            
            // Используем reflection чтобы получить Products из анонимного объекта
            var productsProperty = response.GetType().GetProperty("Products");
            var products = (IEnumerable<Product>)productsProperty.GetValue(response);
            
            Assert.Equal(2, products.Count()); // Только активные товары (не включая неактивный)
        }

        [Fact]
        public async Task GetProducts_WithCategoryFilter_ReturnsFilteredProducts()
        {
            // Act
            var result = await _controller.GetProducts(category: "Electronics");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = okResult.Value;
            
            var productsProperty = response.GetType().GetProperty("Products");
            var products = (IEnumerable<Product>)productsProperty.GetValue(response);
            
            Assert.All(products, p => Assert.Contains("Electronics", p.Category));
        }

        [Fact]
        public async Task GetProducts_WithSearchQuery_ReturnsMatchingProducts()
        {
            // Act
            var result = await _controller.GetProducts(search: "iPhone");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = okResult.Value;
            
            var productsProperty = response.GetType().GetProperty("Products");
            var products = (IEnumerable<Product>)productsProperty.GetValue(response);
            
            Assert.Single(products);
            Assert.Contains("iPhone", products.First().Name);
        }

        [Fact]
        public async Task GetProducts_WithPagination_ReturnsCorrectPage()
        {
            // Act
            var result = await _controller.GetProducts(page: 1, pageSize: 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = okResult.Value;
            
            var productsProperty = response.GetType().GetProperty("Products");
            var products = (IEnumerable<Product>)productsProperty.GetValue(response);
            
            Assert.Single(products); // Только 1 товар на странице
            
            var totalCountProperty = response.GetType().GetProperty("TotalCount");
            var totalCount = (int)totalCountProperty.GetValue(response);
            Assert.Equal(2, totalCount); // Общее количество активных товаров
        }

        [Fact]
        public async Task GetProduct_WithValidId_ReturnsProduct()
        {
            // Act
            var result = await _controller.GetProduct(1);

            // Assert
            var okResult = Assert.IsType<ActionResult<Product>>(result);
            var product = okResult.Value;
            
            Assert.NotNull(product);
            Assert.Equal(1, product.Id);
            Assert.Equal("Test iPhone", product.Name);
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetProduct(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetProduct_WithInactiveProduct_ReturnsNotFound()
        {
            // Act - пытаемся получить неактивный товар
            var result = await _controller.GetProduct(3);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_WithValidData_CreatesProduct()
        {
            // Arrange
            var newProduct = new Product
            {
                Name = "New Test Product",
                Description = "New test description",
                Price = 25000m,
                StockQuantity = 15,
                Category = "Test Category",
                Brand = "Test Brand",
                IsActive = true
            };

            // Act
            var result = await _controller.CreateProduct(newProduct);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdProduct = Assert.IsType<Product>(createdResult.Value);
            
            Assert.Equal(newProduct.Name, createdProduct.Name);
            Assert.True(createdProduct.Id > 0); // ID был присвоен
            
            // Проверяем что товар действительно добавлен в БД
            var productInDb = await _context.Products.FindAsync(createdProduct.Id);
            Assert.NotNull(productInDb);
        }

        [Fact]
        public async Task UpdateProduct_WithValidData_UpdatesProduct()
        {
            // Arrange
            var existingProduct = await _context.Products.FindAsync(1);
            existingProduct.Name = "Updated iPhone";
            existingProduct.Price = 55000m;

            // Act
            var result = await _controller.UpdateProduct(1, existingProduct);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Проверяем что товар обновился в БД
            var updatedProduct = await _context.Products.FindAsync(1);
            Assert.Equal("Updated iPhone", updatedProduct.Name);
            Assert.Equal(55000m, updatedProduct.Price);
        }

        [Fact]
        public async Task UpdateProduct_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test" };

            // Act - передаем ID=2, а в объекте ID=1
            var result = await _controller.UpdateProduct(2, product);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var product = new Product { Id = 999, Name = "Test" };

            // Act
            var result = await _controller.UpdateProduct(999, product);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_WithValidId_SoftDeletesProduct()
        {
            // Act
            var result = await _controller.DeleteProduct(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Проверяем что товар помечен как неактивный (soft delete)
            var deletedProduct = await _context.Products.FindAsync(1);
            Assert.NotNull(deletedProduct); // Товар не удален физически
            Assert.False(deletedProduct.IsActive); // Но помечен как неактивный
        }

        [Fact]
        public async Task DeleteProduct_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteProduct(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetCategories_ReturnsDistinctActiveCategories()
        {
            // Act
            var result = await _controller.GetCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categories = Assert.IsType<List<string>>(okResult.Value);
            
            Assert.Single(categories); // Только одна категория "Electronics" (из активных товаров)
            Assert.Contains("Electronics", categories);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}