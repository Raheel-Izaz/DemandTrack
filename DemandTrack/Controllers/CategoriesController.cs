using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemandTrack.Data;
using DemandTrack.Dtos;
using DemandTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemandTrack.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] bool activeOnly = true)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .AsQueryable();

            if (activeOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    SubCategoriesCount = c.SubCategories.Count,
                    ItemsCount = c.Products.Count
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                SubCategoriesCount = category.SubCategories.Count,
                ItemsCount = category.Products.Count
            };

            return Ok(result);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Category name is required." });
            }

            if (dto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId.Value);
                if (!parentExists)
                {
                    return BadRequest(new { message = $"Parent category with ID {dto.ParentCategoryId.Value} does not exist." });
                }
            }

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Code = dto.Code?.Trim(),
                Description = dto.Description?.Trim(),
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                SubCategoriesCount = 0,
                ItemsCount = 0
            });
        }

        // PUT: api/categories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId.Value == id)
            {
                return BadRequest(new { message = "A category cannot be its own parent." });
            }

            category.Name = dto.Name.Trim();
            category.Code = dto.Code?.Trim();
            category.Description = dto.Description?.Trim();
            category.ParentCategoryId = dto.ParentCategoryId;
            category.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }

            if (category.Products.Any() || category.SubCategories.Any())
            {
                // Soft delete by deactivating to prevent foreign key breakages
                category.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Category has linked items or subcategories, so it was marked as inactive instead of permanently deleted." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
