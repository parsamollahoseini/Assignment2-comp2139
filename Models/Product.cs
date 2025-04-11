using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty; // Initialize non-nullable string

    [Required(ErrorMessage = "Please select a category")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int LowStockThreshold { get; set; }

    // Navigation property
    public virtual Category Category { get; set; } = null!; // Initialize non-nullable navigation property
}
