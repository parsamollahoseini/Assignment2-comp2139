using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventory.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        
        public Order Order { get; set; } = null!; // Initialize non-nullable navigation property
        
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        
        public Product Product { get; set; } = null!; // Initialize non-nullable navigation property
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
