using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Infrastructure.Data;
using ReStore.Core.Entities; 

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // 1. هنا بنطلب من الـ API يدينا نسخة من الداتا بيز بتاعتنا
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 2. دي الـ Endpoint اللي هترجع الأقسام للـ Front-end
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            // بنروح لجدول الأقسام، ونجيب كل الداتا اللي جواه ونحولها لليستة
            var categories = await _context.Categories.ToListAsync();
            
            // بنرجع الداتا دي بـ Status Code 200 (يعني كله تمام OK)
            return Ok(categories);
        }

        // دي Endpoint لإضافة قسم جديد
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Category name is required." });

            var newCategory = new Category
            {
                Name = category.Name
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return Ok(newCategory);
        }

        // دي Endpoint لتعديل قسم موجود
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

        // دي Endpoint لحذف قسم
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