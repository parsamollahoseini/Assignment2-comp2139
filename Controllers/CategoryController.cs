using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Add this line
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.Models;
using Microsoft.Extensions.Logging; // Add this for ILogger

namespace SmartInventory.Controllers
{
    [Authorize(Roles = "Admin")] // Restrict all actions in this controller to Admins
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger; // Add logger

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger) // Inject logger
        {
            _context = context;
            _logger = logger; // Assign logger
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category '{CategoryName}' created successfully.", category.Name);
                    TempData["StatusMessage"] = $"Category '{category.Name}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating category '{CategoryName}'", category.Name);
                    ModelState.AddModelError("", "An error occurred while creating the category.");
                }
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Category ID {CategoryId} updated successfully.", category.CategoryId);
                    TempData["StatusMessage"] = $"Category '{category.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Concurrency error updating category ID {CategoryId}", category.CategoryId);
                    if (!CategoryExists(category.CategoryId))
                    {
                        _logger.LogWarning("Category ID {CategoryId} not found during concurrency check.", category.CategoryId);
                        return NotFound();
                    }
                    else
                    {
                         ModelState.AddModelError("", "The category was modified by another user. Please reload and try again.");
                         // We don't re-throw here, let the user see the error on the Edit page
                    }
                }
                 catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating category ID {CategoryId}", category.CategoryId);
                    ModelState.AddModelError("", "An error occurred while updating the category.");
                    // Don't redirect here, let the user see the error on the Edit page
                    return View(category);
                }
                // Only redirect if save was successful and no concurrency error occurred that wasn't handled by returning the view
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            string categoryName = category?.Name ?? "Unknown"; // Get name before potential deletion

            if (category == null)
            {
                 _logger.LogWarning("Attempted to delete non-existent category ID {CategoryId}", id);
                 TempData["ErrorMessage"] = "Category not found.";
                 return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) deleted successfully.", categoryName, id);
                TempData["StatusMessage"] = $"Category '{categoryName}' deleted successfully.";
            }
            catch (DbUpdateException ex)
            {
                // This likely means the category is referenced by products
                _logger.LogError(ex, "Database error deleting category ID {CategoryId} ('{CategoryName}'). It might be referenced by products.", id, categoryName);
                TempData["ErrorMessage"] = $"Error deleting category '{categoryName}'. It might be associated with existing products.";
                // Redirect back to index or delete page
                return RedirectToAction(nameof(Index)); // Or RedirectToAction(nameof(Delete), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting category ID {CategoryId} ('{CategoryName}')", id, categoryName);
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the category.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
