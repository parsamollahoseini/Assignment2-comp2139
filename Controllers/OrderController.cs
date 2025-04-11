using Microsoft.AspNetCore.Authorization; // Add this
using Microsoft.AspNetCore.Identity; // Add this for UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Add this for ILogger

namespace SmartInventory.Controllers
{
    [Authorize] // Require login for all order actions
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrderController> _logger; // Add logger

        public OrderController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<OrderController> logger) // Inject logger
        {
            _context = context;
            _userManager = userManager;
            _logger = logger; // Assign logger
        }

        // GET: /Order/Create
        public async Task<IActionResult> Create()
        {
            // Display products so the guest can select quantities
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int[] productIds, int[] quantities) // Removed guestName, guestEmail
        {
            // Get the current user's ID
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                // Should not happen if [Authorize] is working, but good practice to check
                ModelState.AddModelError("", "User not found. Please log in again.");
                // Consider redirecting to login or showing an error view
            }

            if (productIds == null || quantities == null || productIds.Length != quantities.Length || !productIds.Any())
                ModelState.AddModelError("", "Invalid order submission. Please select products and quantities.");

            if (!ModelState.IsValid)
            {
                var products = await _context.Products.Include(p => p.Category).ToListAsync();
                return View(products);
            }

            // Create a new order with UTC date and link to the user
            Order order = new Order
            {
                UserId = userId, // Assign the logged-in user's ID
                OrderDate = DateTime.UtcNow,  // Use UTC
                OrderDetails = new List<OrderDetail>()
            };

            decimal totalPrice = 0;
            for (int i = 0; i < productIds.Length; i++)
            {
                if (quantities[i] > 0)
                {
                    var product = await _context.Products.FindAsync(productIds[i]);
                    if (product != null)
                    {
                        OrderDetail detail = new OrderDetail
                        {
                            ProductId = product.ProductId,
                            Quantity = quantities[i],
                            UnitPrice = product.Price
                        };
                        totalPrice += product.Price * quantities[i];
                        order.OrderDetails.Add(detail);
                    }
                }
            }
            order.TotalPrice = totalPrice;

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                // Optionally add success message to TempData
                TempData["StatusMessage"] = "Order placed successfully!";
                return RedirectToAction("Summary", new { id = order.OrderId });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error placing order for user {UserId}", userId); // Use logger
                ModelState.AddModelError("", "An error occurred while placing your order. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error placing order for user {UserId}", userId); // Use logger
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
            }

            // If we got here, something failed, redisplay form with selected products
            var productsForView = await _context.Products.Include(p => p.Category).ToListAsync();
            // You might want to repopulate the quantities if possible, or just show the form again
            return View(productsForView); // Use the renamed variable
        }

        // GET: /Order/Summary/5
        public async Task<IActionResult> Summary(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User) // Include the User data
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}
