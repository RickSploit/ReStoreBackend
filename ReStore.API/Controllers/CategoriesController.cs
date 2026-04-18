using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Infrastructure.Data;
using ReStore.Core.Entities;
using ReStore.API.DTOs;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            
            return Ok(categories);
        }

        [HttpPost]
        [HttpPost]
public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryCreateDto categoryDto)
{
    var category = new Category
    {
        Name = categoryDto.Name,
        
    };

    _context.Categories.Add(category);
    await _context.SaveChangesAsync();

    var resultDto = new CategoryDto
    {
        Id = category.Id,
        Name = category.Name,
        
    };

    return Ok(resultDto);
}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updatedCategory)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (string.IsNullOrWhiteSpace(updatedCategory.Name))
                return BadRequest(new { message = "Category name is required." });

            category.Name = updatedCategory.Name;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Category updated successfully.", category });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}