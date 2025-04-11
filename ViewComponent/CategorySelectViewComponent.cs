// In a file named CategorySelectViewComponent.cs in a ViewComponents folder
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;

public class CategorySelectViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CategorySelectViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        var selectList = new SelectList(categories, "CategoryId", "Name");
        return View(selectList);
    }
}