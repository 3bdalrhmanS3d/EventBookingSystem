using System;
using System.Linq;
using System.Threading.Tasks;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.DTOs;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventBookingSystemV1.Controllers
{
    [Route("categories")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class CategoryController : BaseController
    {
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<CategoryController> logger
        ) : base(context, env)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /categories
        [HttpGet("")]
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            _logger.LogInformation("Category.Index called by {User} (search={Search}, page={Page})",
                User.Identity?.Name, search, page);

            var query = _context.EventCategories
                                .AsNoTracking()
                                .OrderBy(c => c.Name)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.Name, $"%{search.Trim()}%"));
            }

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
        public IActionResult CreateCategory()
        {
            _logger.LogInformation("Category.CreateCategory (GET) called by {User}", User.Identity?.Name);
            return View(new CategoryDto());
        }

        // POST: /categories/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryDto dto)
        {
            _logger.LogInformation("Category.CreateCategory (POST) with Name={Name}", dto.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Category.CreateCategory: ModelState invalid");
                return View(dto);
            }

            var exists = await _context.EventCategories
                .AnyAsync(c => c.Name.ToLower().Trim() == dto.Name.ToLower().Trim());
            if (exists)
            {
                _logger.LogWarning("Category.CreateCategory: duplicate name {Name}", dto.Name);
                ModelState.AddModelError(nameof(dto.Name), "Category with this name already exists.");
                return View(dto);
            }

            var category = new EventCategory { Name = dto.Name.Trim() };
            try
            {
                _context.EventCategories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category added successfully!";
                await LogAuditAsync(nameof(EventCategory), category.Id, "Create", new { category.Name });

                _logger.LogInformation("Category.CreateCategory: created Id={Id}", category.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Category.CreateCategory: error saving new category");
                ModelState.AddModelError("", "Unexpected error occurred. Please try again.");
                return View(dto);
            }
        }

        // GET: /categories/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> EditCategory(int id)
        {
            _logger.LogInformation("Category.EditCategory (GET) for Id={Id}", id);

            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category.EditCategory: Id={Id} not found", id);
                return NotFound();
            }

            var dto = new CategoryDto { Id = category.Id, Name = category.Name };
            return View(dto);
        }

        // POST: /categories/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, CategoryDto dto)
        {
            _logger.LogInformation("Category.EditCategory (POST) Id={Id}, Name={Name}", id, dto.Name);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Category.EditCategory: ModelState invalid for Id={Id}", id);
                return View(dto);
            }

            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category.EditCategory: Id={Id} not found", id);
                return NotFound();
            }

            category.Name = dto.Name.Trim();
            try
            {
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category updated successfully!";
                await LogAuditAsync(nameof(EventCategory), category.Id, "Update", new { category.Name });

                _logger.LogInformation("Category.EditCategory: updated Id={Id}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Category.EditCategory: error updating Id={Id}", id);
                ModelState.AddModelError("", "Unexpected error occurred. Please try again.");
                return View(dto);
            }
        }

        // POST: /categories/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            _logger.LogInformation("Category.DeleteCategory called for Id={Id}", id);

            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category.DeleteCategory: Id={Id} not found", id);
                return NotFound();
            }

            try
            {
                _context.EventCategories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category deleted successfully!";
                await LogAuditAsync(nameof(EventCategory), category.Id, "Delete", new { category.Name });

                _logger.LogInformation("Category.DeleteCategory: deleted Id={Id}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Category.DeleteCategory: error deleting Id={Id}", id);
                TempData["ErrorMessage"] = "Unable to delete category. Try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Category.Details called for Id={Id}", id);

            var category = await _context.EventCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                _logger.LogWarning("Category.Details: Id={Id} not found", id);
                return NotFound();
            }

            var vm = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };
            return View(vm);
        }
    }
}
