using Xunit;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Controllers;
using ECommerceApi.Models;
using ECommerceApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Tests.Controllers
{
    public class UsersControllerTests : IDisposable
    {
        private readonly UsersController _controller;
        private readonly ECommerceApi.Data.ECommerceDbContext _context;

        public UsersControllerTests()
        {
            _context = TestDbContextFactory.CreateContextWithTestData().Result;
            _controller = new UsersController(_context);
        }

        [Fact]
        public async Task GetUser_WithValidId_ReturnsUser()
        {
            // Act
            var result = await _controller.GetUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userInfo = okResult.Value;
            
            // Проверяем что пароль не возвращается
            var idProperty = userInfo.GetType().GetProperty("Id");
            var emailProperty = userInfo.GetType().GetProperty("Email");
            var passwordProperty = userInfo.GetType().GetProperty("PasswordHash");
            
            Assert.Equal(1, idProperty.GetValue(userInfo));
            Assert.Equal("test@example.com", emailProperty.GetValue(userInfo));
            Assert.Null(passwordProperty); // Пароль не должен возвращаться
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetUser(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Register_WithValidData_CreatesUser()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                FirstName = "New",
                LastName = "User",
                Email = "newuser@example.com",
                Password = "password123",
                Phone = "+7 (999) 123-45-67"
            };

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var userInfo = createdResult.Value;
            
            var emailProperty = userInfo.GetType().GetProperty("Email");
            var isAdminProperty = userInfo.GetType().GetProperty("IsAdmin");
            
            Assert.Equal("newuser@example.com", emailProperty.GetValue(userInfo));
            Assert.False((bool)isAdminProperty.GetValue(userInfo)); // Новый пользователь не админ
            
            // Проверяем что пользователь создался в БД
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            Assert.NotNull(userInDb);
            Assert.NotEqual("password123", userInDb.PasswordHash); // Пароль должен быть захеширован
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange - пытаемся зарегистрироваться с существующим email
            var registerRequest = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com", // Email уже существует
                Password = "password123"
            };

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("User with this email already exists", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "hashedpassword" // Тестовый пароль из тестовых данных (не хешированный для теста)
            };

            // Поскольку тестовые данные используют прямой хеш, создадим пользователя с правильным хешом
            var testUser = await _context.Users.FirstAsync(u => u.Email == "test@example.com");
            testUser.PasswordHash = _controller.GetType()
                .GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_controller, new[] { "testpassword" })?.ToString() ?? "";
            await _context.SaveChangesAsync();

            loginRequest.Password = "testpassword";

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userInfo = okResult.Value;
            
            var emailProperty = userInfo.GetType().GetProperty("Email");
            var messageProperty = userInfo.GetType().GetProperty("Message");
            
            Assert.Equal("test@example.com", emailProperty.GetValue(userInfo));
            Assert.Equal("Login successful", messageProperty.GetValue(userInfo));
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid email or password", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid email or password", unauthorizedResult.Value);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_UpdatesUser()
        {
            // Arrange
            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@example.com",
                Phone = "+7 (888) 999-00-11"
            };

            // Act
            var result = await _controller.UpdateUser(1, updateRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Проверяем что данные обновились в БД
            var updatedUser = await _context.Users.FindAsync(1);
            Assert.Equal("Updated", updatedUser.FirstName);
            Assert.Equal("Name", updatedUser.LastName);
            Assert.Equal("updated@example.com", updatedUser.Email);
            Assert.Equal("+7 (888) 999-00-11", updatedUser.Phone);
        }

        [Fact]
        public async Task UpdateUser_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange - пытаемся изменить email на уже существующий
            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "admin@example.com", // Email второго пользователя
                Phone = "+7 (999) 123-45-67"
            };

            // Act
            var result = await _controller.UpdateUser(1, updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User with this email already exists", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUser_WithNonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };

            // Act
            var result = await _controller.UpdateUser(999, updateRequest);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUsers_ReturnsPaginatedUsers()
        {
            // Act
            var result = await _controller.GetUsers(page: 1, pageSize: 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = okResult.Value;
            
            var usersProperty = response.GetType().GetProperty("Users");
            var users = (IEnumerable<object>)usersProperty.GetValue(response);
            
            var totalCountProperty = response.GetType().GetProperty("TotalCount");
            var totalCount = (int)totalCountProperty.GetValue(response);
            
            Assert.Single(users); // Только 1 пользователь на странице
            Assert.Equal(2, totalCount); // Всего 2 пользователя в тестовых данных
        }

        [Fact]
        public async Task ToggleAdminStatus_WithValidUser_UpdatesAdminStatus()
        {
            // Arrange
            var toggleRequest = new ToggleAdminRequest { IsAdmin = true };

            // Act - делаем обычного пользователя админом
            var result = await _controller.ToggleAdminStatus(1, toggleRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // Проверяем что статус изменился в БД
            var updatedUser = await _context.Users.FindAsync(1);
            Assert.True(updatedUser.IsAdmin);
        }

        [Fact]
        public async Task ToggleAdminStatus_WithNonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var toggleRequest = new ToggleAdminRequest { IsAdmin = true };

            // Act
            var result = await _controller.ToggleAdminStatus(999, toggleRequest);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}