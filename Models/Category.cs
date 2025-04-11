using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; // Initialize non-nullable string
        public string Description { get; set; } = string.Empty; // Initialize non-nullable string

        // Navigation property
        public ICollection<Product> Products { get; set; }

        // Constructor to initialize the collection
        public Category()
        {
            Products = new HashSet<Product>(); // Initialize collection
        }
    }
}
