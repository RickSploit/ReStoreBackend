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
public async Task<IActionResult> GetAppliances()
{
    // 1. نجيب كل الأجهزة من الداتا بيز
    var appliances = await _context.Appliances.ToListAsync();

    // 2. السطر ده بيجيب عنوان السيرفر الحالي (مثلاً https://localhost:5104)
    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

    // 3. نحول الـ Entity المعقد لـ DTO نضيف، ونظبط لينك الصورة
    var applianceDtos = appliances.Select(a => new ApplianceDto
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        Price = a.Price,
        Condition = a.Condition.ToString(), // بيحول الـ Enum لكلمة مقروءة
        
        // لو في صورة، الزق الـ baseUrl في مسار الصورة، لو مفيش رجع null
        ImageUrl = string.IsNullOrEmpty(a.ImageUrl) ? null : $"{baseUrl}{a.ImageUrl}"
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