using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => Quantity * UnitPrice;
        
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}