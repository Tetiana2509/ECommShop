using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public User User { get; set; }
        public Product Product { get; set; }
    }
}