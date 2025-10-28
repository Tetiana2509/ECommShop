using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsAdmin { get; set; } = false;
        
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}