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
    }
}