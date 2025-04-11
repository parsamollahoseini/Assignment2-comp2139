using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Add this for ForeignKey
using SmartInventory.Models;

namespace SmartInventory.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }

        // Foreign Key for the user who placed the order
        [Required]
        public string UserId { get; set; } = string.Empty; // Initialize non-nullable string

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!; // Initialize non-nullable navigation property

        // Navigation property for order details
        public ICollection<OrderDetail> OrderDetails { get; set; }

        // Constructor to initialize the collection
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>(); // Initialize collection
        }
    }
}
