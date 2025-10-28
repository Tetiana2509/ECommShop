using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;

namespace ECommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public OrdersController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.TotalAmount,
                    o.Status,
                    o.ShippingAddress,
                    o.CreatedAt,
                    o.UpdatedAt,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.Quantity,
                        oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice,
                        Product = new
                        {
                            oi.Product.Id,
                            oi.Product.Name,
                            oi.Product.ImageUrl
                        }
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = new
            {
                order.Id,
                order.TotalAmount,
                order.Status,
                order.ShippingAddress,
                order.CreatedAt,
                order.UpdatedAt,
                User = new
                {
                    order.User.Id,
                    order.User.FirstName,
                    order.User.LastName,
                    order.User.Email
                },
                Items = order.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.Quantity,
                    oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice,
                    Product = new
                    {
                        oi.Product.Id,
                        oi.Product.Name,
                        oi.Product.ImageUrl,
                        oi.Product.Description
                    }
                }).ToList()
            };

            return Ok(orderDetails);
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateOrder(CreateOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == request.UserId)
                    .Include(c => c.Product)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return BadRequest("Warenkorb ist leer");
                }

                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Product.StockQuantity < cartItem.Quantity)
                    {
                        return BadRequest($"Nicht genügend Lagerbestand für Produkt: {cartItem.Product.Name}");
                    }
                }

                var order = new Order
                {
                    UserId = request.UserId,
                    ShippingAddress = request.ShippingAddress,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                decimal totalAmount = 0;
                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price
                    };

                    _context.OrderItems.Add(orderItem);
                    totalAmount += orderItem.Quantity * orderItem.UnitPrice;

                    cartItem.Product.StockQuantity -= cartItem.Quantity;
                    cartItem.Product.UpdatedAt = DateTime.UtcNow;
                }

                order.TotalAmount = totalAmount;

                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
                {
                    order.Id,
                    order.TotalAmount,
                    order.Status,
                    Message = "Bestellung erfolgreich erstellt"
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Fehler beim Erstellen der Bestellung");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Bestellstatus erfolgreich aktualisiert" });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllOrders(
            [FromQuery] OrderStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Orders.Include(o => o.User).AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.TotalAmount,
                    o.Status,
                    o.CreatedAt,
                    User = new
                    {
                        o.User.FirstName,
                        o.User.LastName,
                        o.User.Email
                    }
                })
                .ToListAsync();

            var result = new
            {
                Orders = orders,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }
    }

    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
    }

    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}