using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public UsersController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userInfo = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsAdmin,
                user.CreatedAt
            };

            return Ok(userInfo);
        }

        [HttpPost("register")]
        public async Task<ActionResult<object>> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Benutzer mit dieser E-Mail existiert bereits");
            }

            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = passwordHash,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userInfo = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsAdmin,
                Message = "Benutzer erfolgreich registriert"
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userInfo);
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized("Ungültige E-Mail oder Passwort");
            }

            var userInfo = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsAdmin,
                Message = "Anmeldung erfolgreich"
            };

            return Ok(userInfo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.UpdatedAt = DateTime.UtcNow;

            if (user.Email != request.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                {
                    return BadRequest("Benutzer mit dieser E-Mail existiert bereits");
                }
                user.Email = request.Email;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var totalCount = await _context.Users.CountAsync();
            var users = await _context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(u => u.LastName)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.IsAdmin,
                    u.CreatedAt
                })
                .ToListAsync();

            var result = new
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        [HttpPut("{id}/admin")]
        public async Task<IActionResult> ToggleAdminStatus(int id, ToggleAdminRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsAdmin = request.IsAdmin;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Benutzer-Administratorstatus auf {request.IsAdmin} aktualisiert" });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashToCheck = HashPassword(password);
            return hashToCheck == hashedPassword;
        }
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class ToggleAdminRequest
    {
        public bool IsAdmin { get; set; }
    }
}