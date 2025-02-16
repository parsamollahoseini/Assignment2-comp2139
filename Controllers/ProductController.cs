using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInventory.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
            // Log the connection string for debugging
            var connectionString = _context.Database.GetDbConnection().ConnectionString;
            Console.WriteLine("ProductController using connection string: " + connectionString);
        }

        // GET: /Product
        public async Task<IActionResult> Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            // Start with all products including their Category
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(searchString))
                products = products.Where(p => p.Name.Contains(searchString));
            if (categoryId.HasValue && categoryId.Value > 0)
                products = products.Where(p => p.CategoryId == categoryId);
            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice);
            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice);

            // Apply sorting
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParam"] = sortOrder == "Price" ? "price_desc" : "Price";
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "Price":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");

            var productList = await products.ToListAsync();
            Console.WriteLine("Index action: Product count = " + productList.Count);
            return View(productList);
        }

        // GET: /Product/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: /Product/Create
        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Debug logging
            Console.WriteLine("Form data received:");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"CategoryId: {product.CategoryId}");
            Console.WriteLine($"Price: {product.Price}");
            Console.WriteLine($"QuantityInStock: {product.QuantityInStock}");
            Console.WriteLine($"LowStockThreshold: {product.LowStockThreshold}");

            try
            {
                // Explicitly set the state to Added
                _context.Entry(product).State = EntityState.Added;
        
                // Try to save
                await _context.SaveChangesAsync();
        
                // If we get here, save was successful
                Console.WriteLine($"Product saved successfully with ID: {product.ProductId}");
        
                // Check if the product exists in the database
                var savedProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
            
                Console.WriteLine($"Product exists in database: {savedProduct != null}");
        
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", "Failed to save product");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Product/Edit/5
       // GET: /Product/Edit/5
public async Task<IActionResult> Edit(int? id)
{
    if (id == null)
        return NotFound();
        
    var product = await _context.Products
        .Include(p => p.Category)  // Make sure to include Category
        .FirstOrDefaultAsync(p => p.ProductId == id);
        
    if (product == null)
        return NotFound();
    
    // Debug logging
    Console.WriteLine($"Loading product for edit:");
    Console.WriteLine($"ProductId: {product.ProductId}");
    Console.WriteLine($"Name: {product.Name}");
    Console.WriteLine($"CategoryId: {product.CategoryId}");
    Console.WriteLine($"Category Name: {product.Category?.Name}");
        
    // Make sure to select the current category in the dropdown
    ViewBag.Categories = new SelectList(
        await _context.Categories.ToListAsync(),
        "CategoryId", 
        "Name", 
        product.CategoryId);

    return View(product);
}

// POST: /Product/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
                return NotFound();

            Console.WriteLine($"Edit POST received for product:");
            Console.WriteLine($"ProductId: {product.ProductId}");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"CategoryId: {product.CategoryId}");
            Console.WriteLine($"Price: {product.Price}");
            Console.WriteLine($"QuantityInStock: {product.QuantityInStock}");
            Console.WriteLine($"LowStockThreshold: {product.LowStockThreshold}");

            try
            {
                // Explicitly set the state to Modified
                _context.Entry(product).State = EntityState.Modified;
        
                // Try to save
                var changes = await _context.SaveChangesAsync();
        
                // Log the result
                Console.WriteLine($"SaveChangesAsync returned: {changes} changes");
        
                // Verify the update
                var updatedProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
            
                Console.WriteLine($"Product updated in database: {updatedProduct != null}");
                if (updatedProduct != null)
                {
                    Console.WriteLine($"Updated values - Name: {updatedProduct.Name}, CategoryId: {updatedProduct.CategoryId}");
                }
        
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", "Failed to update product");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Product/Delete/
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
                
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
                return NotFound();
                
            return View(product);
        }

        // POST: /Product/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
