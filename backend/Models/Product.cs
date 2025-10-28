using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        public int StockQuantity { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; }
        
        [StringLength(50)]
        public string? Brand { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}