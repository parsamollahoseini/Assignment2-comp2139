using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; }
    }
}