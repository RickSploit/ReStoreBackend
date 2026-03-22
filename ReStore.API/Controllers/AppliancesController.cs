using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services; // عشان يشوف خدمة الصور
using ReStore.API.DTOs;

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

        // جلب كل الأجهزة
        [HttpGet]
[HttpGet]
public async Task<IActionResult> GetAppliances(
    [FromQuery] string? search,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice)
{
    var query = _context.Appliances.AsQueryable();

    // Search باسم الجهاز
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(a => a.Title.Contains(search));
    }

    // Filter بأقل سعر
    if (minPrice.HasValue)
    {
        query = query.Where(a => a.Price >= minPrice.Value);
    }

    // Filter بأعلى سعر
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
        ImageUrl = $"{baseUrl}{a.ImageUrl}"
    }).ToList();

    return Ok(applianceDtos);
}

        // إضافة جهاز جديد بالصورة بتاعته
        [HttpPost]
public async Task<IActionResult> CreateAppliance([FromForm] ApplianceCreateDto dto, IFormFile file)
{
    // 1. بناخد البيانات النظيفة من الـ DTO ونحطها في الـ Entity بتاع الداتا بيز
    var appliance = new Appliance
    {
        Title = dto.Title,
        Description = dto.Description,
        Price = dto.Price,
        CategoryId = dto.CategoryId,
        SellerId = dto.SellerId,
        Condition = (ApplianceCondition)dto.Condition
    };

    // 2. لو في صورة مبعوتة، نرفعها
    if (file != null)
    {
        var imageUrl = await _imageService.UploadImageAsync(file);
        appliance.ImageUrl = imageUrl;
    }

    // 3. نحفظ في الداتا بيز
    _context.Appliances.Add(appliance);
    await _context.SaveChangesAsync();

    return Ok(appliance); // ممكن نرجع DTO تاني للـ Get بعدين، بس خلينا كدة دلوقتي
}

// تعديل جهاز موجود
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppliance(int id, [FromForm] ApplianceCreateDto dto, IFormFile? file)
    {
        var appliance = await _context.Appliances.FindAsync(id);

        if (appliance == null)
            return NotFound(new { message = "Appliance not found" });

        // تحديث البيانات
        appliance.Title = dto.Title;
        appliance.Description = dto.Description;
        appliance.Price = dto.Price;
        appliance.CategoryId = dto.CategoryId;
        appliance.SellerId = dto.SellerId;
        appliance.Condition = (ApplianceCondition)dto.Condition;

        // لو المستخدم بعت صورة جديدة، نرفعها ونحدث ImageUrl
        if (file != null)
        {
            var imageUrl = await _imageService.UploadImageAsync(file);
            appliance.ImageUrl = imageUrl;
        }

        _context.Appliances.Update(appliance);
        await _context.SaveChangesAsync();

        return Ok(appliance);
    }

    // حذف جهاز
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