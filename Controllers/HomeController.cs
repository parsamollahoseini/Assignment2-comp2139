using Microsoft.AspNetCore.Mvc;
using SmartInventory.Data;
using System.Linq;
using SmartInventory.Data;

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
    }
}