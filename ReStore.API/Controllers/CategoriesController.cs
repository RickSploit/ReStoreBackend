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
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategories()
        {
            // بنجيب الداتا ونحولها لـ DTO عشان السواجر والفرونت إند
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
            
            return Ok(categories);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // استخدمنا DTO هنا بدل Entity عشان نحمي الكود
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto updatedCategory)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (string.IsNullOrWhiteSpace(updatedCategory.Name))
                return BadRequest(new { message = "Category name is required." });

            category.Name = updatedCategory.Name;

            await _context.SaveChangesAsync();

            // حولنا النتيجة لـ DTO عشان نتجنب إيرور الـ Object Cycle
            var resultDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            return Ok(new { message = "Category updated successfully.", category = resultDto });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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