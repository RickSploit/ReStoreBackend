using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services; 
using ReStore.API.DTOs;
using ReStore.API.Entities;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppliancesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageService _imageService;

        public AppliancesController(ApplicationDbContext context, ImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppliances(
            [FromQuery] string? search,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            // التعديل الأول: ضفنا Include عشان السيرفر يروح يجيب الصور المرتبطة بالجهاز
            var query = _context.Appliances.Include(a => a.Images).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Title.Contains(search));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(a => a.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(a => a.Price <= maxPrice.Value);
            }

            var appliances = await query.ToListAsync();
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var applianceDtos = appliances.Select(a => new ApplianceDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Price = a.Price,
                Condition = a.Condition.ToString(),
                // التعديل التاني: بنجيب أول صورة من لستة الصور، ولو مفيش بنرجع null
                ImageUrl = a.Images.FirstOrDefault(i => i.IsMain)?.Url != null 
                           ? $"{baseUrl}{a.Images.FirstOrDefault(i => i.IsMain)?.Url}" 
                           : null
            }).ToList();

            return Ok(applianceDtos);
        }
        [HttpGet("{id}")]
public async Task<ActionResult<ApplianceDto>> GetApplianceDetails(int id)
{
    var appliance = await _context.Appliances
        .Include(a => a.Images)
        .Include(a => a.Category)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (appliance == null)
        return NotFound(new { message = "الجهاز غير موجود" });

    // تحويل الـ Entity لـ DTO عشان نرجعه للفرونت
    var applianceDto = new ApplianceDto 
    {
        Id = appliance.Id,
        Title = appliance.Title,
        Description = appliance.Description,
        Price = appliance.Price,
        CategoryName = appliance.Category.Name,
        // بنجيب كل صور الجهاز عشان تظهر في الجاليري بتاع صفحة التفاصيل
        ImageUrls = appliance.Images.Select(i => i.Url).ToList() 
    };

    return Ok(applianceDto);
}

        [HttpPost]
        public async Task<IActionResult> CreateAppliance([FromForm] ApplianceCreateDto dto, IFormFile file)
        {
            var appliance = new Appliance
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                SellerId = dto.SellerId,
                Condition = (ApplianceCondition)dto.Condition
            };

            // نحفظ الجهاز الأول عشان ياخد ID
            _context.Appliances.Add(appliance);
            await _context.SaveChangesAsync();

            // التعديل التالت: بنرفع الصورة ونحفظها في جدول الصور الجديد ونربطها بـ ID الجهاز
            if (file != null)
            {
                var imageUrl = await _imageService.UploadImageAsync(file);
                var applianceImage = new ApplianceImage
                {
                    Url = imageUrl,
                    IsMain = true,
                    ApplianceId = appliance.Id
                };
                _context.ApplianceImages.Add(applianceImage);
                await _context.SaveChangesAsync();
            }

            return Ok(appliance); 
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppliance(int id, [FromForm] ApplianceCreateDto dto, IFormFile? file)
        {
            var appliance = await _context.Appliances.FindAsync(id);

            if (appliance == null)
                return NotFound(new { message = "Appliance not found" });

            appliance.Title = dto.Title;
            appliance.Description = dto.Description;
            appliance.Price = dto.Price;
            appliance.CategoryId = dto.CategoryId;
            appliance.SellerId = dto.SellerId;
            appliance.Condition = (ApplianceCondition)dto.Condition;

            _context.Appliances.Update(appliance);
            await _context.SaveChangesAsync();

            // التعديل الرابع: نفس الفكرة في التعديل، لو رفع صورة جديدة بنحطها في جدول الصور
            if (file != null)
            {
                var imageUrl = await _imageService.UploadImageAsync(file);
                var applianceImage = new ApplianceImage
                {
                    Url = imageUrl,
                    IsMain = false, // ممكن نخليها فولس عشان دي صورة إضافية، أو تمسح القديم براحتك
                    ApplianceId = appliance.Id
                };
                _context.ApplianceImages.Add(applianceImage);
                await _context.SaveChangesAsync();
            }

            return Ok(appliance);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppliance(int id)
        {
            var appliance = await _context.Appliances.FindAsync(id);

            if (appliance == null)
                return NotFound(new { message = "Appliance not found" });

            _context.Appliances.Remove(appliance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appliance deleted successfully" });
        }
    }
}