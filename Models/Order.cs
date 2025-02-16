using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartInventory.Models;

namespace SmartInventory.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        [Required]
        public string GuestName { get; set; }
        
        [Required, EmailAddress]
        public string GuestEmail { get; set; }

        // Navigation property
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}