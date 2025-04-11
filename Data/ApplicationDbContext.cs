using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Models;

namespace SmartInventory.Data
{
    // Inherit from IdentityDbContext and specify the custom user class
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data for Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", Description = "Electronic gadgets" },
                new Category { CategoryId = 2, Name = "Clothing", Description = "Apparel and garments" },
                new Category { CategoryId = 3, Name = "Food", Description = "Edible items" }
            );

            // Seed data for Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Smartphone", CategoryId = 1, Price = 500, QuantityInStock = 20, LowStockThreshold = 5 },
                new Product { ProductId = 2, Name = "Laptop", CategoryId = 1, Price = 1200, QuantityInStock = 10, LowStockThreshold = 3 },
                new Product { ProductId = 3, Name = "T-Shirt", CategoryId = 2, Price = 20, QuantityInStock = 50, LowStockThreshold = 10 },
                new Product { ProductId = 4, Name = "Pizza", CategoryId = 3, Price = 15, QuantityInStock = 30, LowStockThreshold = 5 }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
