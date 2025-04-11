using Microsoft.AspNetCore.Mvc;
using SmartInventory.Data;
using System.Linq;
using SmartInventory.Models; // Add this for ErrorViewModel
using System.Diagnostics; // Add this for Activity
using Microsoft.AspNetCore.Authorization; // Allow anonymous access to error page

namespace SmartInventory.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            // Summary information for the homepage
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalCategories = _context.Categories.Count();
            ViewBag.LowStockProducts = _context.Products.Count(p => p.QuantityInStock < p.LowStockThreshold);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous] // Ensure even unauthenticated users see error pages
        [Route("/Home/Error/{statusCode?}")] // Add route to accept optional status code
        public IActionResult Error(int? statusCode = null) // Accept optional status code
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode // Pass the status code to the view model
            };

            // Optionally, you could add specific logic or logging based on the statusCode here
            // e.g., if (statusCode == 404) { ... }

            return View(errorViewModel);
        }
    }
}
