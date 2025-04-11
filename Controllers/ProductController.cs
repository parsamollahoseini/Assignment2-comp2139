using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Add this line
using SmartInventory.Data;
using SmartInventory.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Add this for ILogger

namespace SmartInventory.Controllers
{
    [Authorize] // Require login for all actions in this controller
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger; // Add logger

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger) // Inject logger
        {
            _context = context;
            _logger = logger; // Assign logger
            
            try
            {
                if (_context.Database.ProviderName?.Contains("InMemory") == false)
                {
                    var connectionString = _context.Database.GetDbConnection().ConnectionString;
                    _logger.LogDebug("ProductController using connection string: {ConnectionString}", connectionString);
                }
                else
                {
                    _logger.LogDebug("ProductController using InMemory database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to determine database connection details");
            }
        }

        // GET: /Product
        public async Task<IActionResult> Index(
            string searchString = null, 
            int? categoryId = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            string sortOrder = null)
        {
            // Explicitly start with a queryable of products
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);

            // Apply filtering
            if (!string.IsNullOrEmpty(searchString))
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
    
            if (categoryId.HasValue && categoryId.Value > 0)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
    
            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice);
    
            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice);

            // Apply sorting
            productsQuery = sortOrder switch
            {
                "name_desc" => productsQuery.OrderByDescending(p => p.Name),
                "Price" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                _ => productsQuery.OrderBy(p => p.Name)
            };

            // Populate categories
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");

            // Execute the query and get the list
            var productList = await productsQuery.ToListAsync();

            
            bool isAjaxRequest = Request?.Headers != null && 
                                 Request.Headers.ContainsKey("X-Requested-With") && 
                                 Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isAjaxRequest)
            {
                // If AJAX, return only the partial view containing the product list
                return PartialView("_ProductListPartial", productList);
            }

            // If not AJAX, return the full view
            return View(productList);
        }

        // GET: /Product/Create
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admins can access the Create page
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: /Product/Create
        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admins can submit the Create form
        public async Task<IActionResult> Create(Product product)
        {
            // Debug logging
            Console.WriteLine("Form data received:");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"CategoryId: {product.CategoryId}");
            Console.WriteLine($"Price: {product.Price}");
            Console.WriteLine($"QuantityInStock: {product.QuantityInStock}");
            Console.WriteLine($"LowStockThreshold: {product.LowStockThreshold}");

            // Check if ModelState is valid first
            if (!ModelState.IsValid)
            {
                // If AJAX request and invalid model, return errors as JSON
                bool isAjaxRequest = Request?.Headers != null && 
                                     Request.Headers.ContainsKey("X-Requested-With") && 
                                     Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                   
                if (isAjaxRequest)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return Json(new { success = false, errors = errors });
                }
                // If not AJAX, return the view with validation errors
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                return View(product);
            }

            // ModelState is valid, proceed with saving
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

                // If AJAX request, return success JSON
                bool isAjaxRequestSuccess = Request?.Headers != null && 
                                            Request.Headers.ContainsKey("X-Requested-With") && 
                                            Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                          
                if (isAjaxRequestSuccess)
                {
                    return Json(new { success = true, message = $"Product '{product.Name}' created successfully." });
                }

                // If not AJAX, redirect as before
                TempData["StatusMessage"] = $"Product '{product.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving new product named {ProductName}", product.Name);
                var errorMessage = "Failed to save product. Please check the details and try again.";
                ModelState.AddModelError("", errorMessage);

                // If AJAX request, return error JSON
                bool isAjaxRequestError = Request?.Headers != null && 
                                          Request.Headers.ContainsKey("X-Requested-With") && 
                                          Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                        
                if (isAjaxRequestError)
                {
                    // You might want to return a simpler error structure for AJAX
                    return Json(new { success = false, errors = new { general = errorMessage } });
                }
            }

            // If save failed (and not AJAX), return the view with model error
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Product/Edit/5
       // GET: /Product/Edit/5
[Authorize(Roles = "Admin")] // Only Admins can access the Edit page
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
        [Authorize(Roles = "Admin")] // Only Admins can submit the Edit form
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
            catch (DbUpdateConcurrencyException ex)
            {
                 _logger.LogWarning(ex, "Concurrency error updating product ID {ProductId}", product.ProductId);
                 ModelState.AddModelError("", "The product was modified by another user. Please reload and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product ID {ProductId}", product.ProductId); // Use logger
                ModelState.AddModelError("", "Failed to update product. Please check the details and try again.");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Product/Delete/
        [Authorize(Roles = "Admin")] // Only Admins can access the Delete confirmation page
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
        [Authorize(Roles = "Admin")] // Only Admins can confirm deletion
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                // Product might have been deleted already
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                // Optionally add a success message to TempData
                TempData["StatusMessage"] = $"Product '{product.Name}' deleted successfully.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting product ID {ProductId}. It might be referenced by orders.", id); // Use logger
                TempData["ErrorMessage"] = "Error deleting product. It might be associated with existing orders.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unexpected error deleting product ID {ProductId}", id); // Use logger
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the product.";
                 return RedirectToAction(nameof(Delete), new { id = id });
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
        
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        
            if (product == null)
                return NotFound();
    
            return View(product);
        }
        
        
    }
}
