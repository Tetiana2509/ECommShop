using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;

namespace ECommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public CartController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCart(int userId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .Select(c => new
                {
                    c.Id,
                    c.Quantity,
                    Product = new
                    {
                        c.Product.Id,
                        c.Product.Name,
                        c.Product.Price,
                        c.Product.ImageUrl,
                        c.Product.StockQuantity
                    },
                    TotalPrice = c.Quantity * c.Product.Price,
                    c.CreatedAt,
                    c.UpdatedAt
                })
                .ToListAsync();

            var cartSummary = new
            {
                Items = cartItems,
                TotalItems = cartItems.Sum(c => c.Quantity),
                TotalAmount = cartItems.Sum(c => c.TotalPrice)
            };

            return Ok(cartSummary);
        }

        [HttpPost]
        public async Task<ActionResult<CartItem>> AddToCart(AddToCartRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null || !product.IsActive)
            {
                return NotFound("Produkt nicht gefunden");
            }

            if (product.StockQuantity < request.Quantity)
            {
                return BadRequest("Nicht genügend Lagerbestand");
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.ProductId == request.ProductId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += request.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = request.UserId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Artikel erfolgreich zum Warenkorb hinzugefügt" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, UpdateCartRequest request)
        {
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (cartItem.Product.StockQuantity < request.Quantity)
            {
                return BadRequest("Nicht genügend Lagerbestand");
            }

            cartItem.Quantity = request.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class AddToCartRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartRequest
    {
        public int Quantity { get; set; }
    }
}