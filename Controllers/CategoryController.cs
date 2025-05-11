using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.DTOs;
using EventBookingSystemV1.ViewModels;
using EventBookingSystemV1.Models;

namespace EventBookingSystemV1.Controllers
{
    [Route("categories")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class CategoryController : BaseController
    {
        public CategoryController(ApplicationDbContext context, IWebHostEnvironment env)
            : base(context, env)
        {
        }

        // GET: /categories
        [HttpGet("")]
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            IQueryable<EventCategory> query = _context.EventCategories
                .AsNoTracking()
                .OrderBy(c => c.Name);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            var total = await query.CountAsync();
            var categories = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;

            return View(categories);
        }

        // GET: /categories/create
        [HttpGet("create")]
        public IActionResult CreateCategory() => View(new CategoryDto());

        // POST: /categories/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (await _context.EventCategories
                .AnyAsync(c => c.Name.ToLower().Trim() == dto.Name.ToLower().Trim()))
            {
                ModelState.AddModelError(nameof(dto.Name), "Category with this name already exists.");
                return View(dto);
            }

            var category = new EventCategory
            {
                Name = dto.Name
            };
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category added successfully!";
            await LogAuditAsync(nameof(EventCategory), category.Id, "Create", new { category.Name });
            return RedirectToAction(nameof(Index));
        }

        // GET: /categories/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
            return View(dto);
        }

        // POST: /categories/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            category.Name = dto.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category updated successfully!";
            await LogAuditAsync(nameof(EventCategory), category.Id, "Update", new { category.Name });
            return RedirectToAction(nameof(Index));
        }

        // POST: /categories/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.EventCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category deleted successfully!";
            await LogAuditAsync(nameof(EventCategory), category.Id, "Delete", new { category.Name });
            return RedirectToAction(nameof(Index));
        }

        // GET: /categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.EventCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return NotFound();

            var vm = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };
            return View(vm);
        }
    }
}
