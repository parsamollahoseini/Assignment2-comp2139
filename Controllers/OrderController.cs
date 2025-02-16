using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartInventory.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create(string guestName, string guestEmail, int[] productIds, int[] quantities)
        {
            if (string.IsNullOrEmpty(guestName) || string.IsNullOrEmpty(guestEmail))
                ModelState.AddModelError("", "Guest name and email are required.");

            if (productIds == null || quantities == null || productIds.Length != quantities.Length)
                ModelState.AddModelError("", "Invalid order submission.");

            if (!ModelState.IsValid)
            {
                var products = await _context.Products.Include(p => p.Category).ToListAsync();
                return View(products);
            }

            // Create a new order with UTC date
            Order order = new Order
            {
                GuestName = guestName,
                GuestEmail = guestEmail,
                OrderDate = DateTime.UtcNow,  // Use UTC to avoid the error
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

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Summary", new { id = order.OrderId });
        }

        // GET: /Order/Summary/5
        public async Task<IActionResult> Summary(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}
