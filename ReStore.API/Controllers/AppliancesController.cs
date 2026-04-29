using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.Infrastructure.Services; 
using ReStore.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
                return NotFound(new { message = "Appliance not found." });

            var applianceDto = new ApplianceDto 
            {
                Id = appliance.Id,
                Title = appliance.Title,
                Description = appliance.Description,
                Price = appliance.Price,
                CategoryName = appliance.Category?.Name,
                ImageUrls = appliance.Images.Select(i => i.Url).ToList() 
            };

            return Ok(applianceDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAppliance([FromForm] ApplianceCreateDto dto)
        {
            // استخدام الفانكشن السحرية الجديدة
            var sellerId = GetCurrentUserId();
            if (sellerId == null) 
                return Unauthorized(new { message = "Invalid token: User ID is missing or incorrect." });

            var appliance = new Appliance
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                SellerId = sellerId.Value,
                Condition = (ApplianceCondition)dto.Condition
            };

            _context.Appliances.Add(appliance);
            await _context.SaveChangesAsync();

            if (dto.Image != null)
            {
                var imageUrl = await _imageService.UploadImageAsync(dto.Image);
                var applianceImage = new ApplianceImage
                {
                    Url = imageUrl,
                    IsMain = true,
                    ApplianceId = appliance.Id
                };
                _context.ApplianceImages.Add(applianceImage);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Appliance created successfully!", applianceId = appliance.Id }); 
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAppliance(int id, [FromForm] ApplianceCreateDto dto)
        {
            var appliance = await _context.Appliances.FindAsync(id);

            if (appliance == null)
                return NotFound(new { message = "Appliance not found" });

            // استخدام الفانكشن السحرية الجديدة للحماية
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null || currentUserId.Value != appliance.SellerId)
                return Forbid();

            appliance.Title = dto.Title;
            appliance.Description = dto.Description;
            appliance.Price = dto.Price;
            appliance.CategoryId = dto.CategoryId;
            appliance.Condition = (ApplianceCondition)dto.Condition;

            _context.Appliances.Update(appliance);
            await _context.SaveChangesAsync();

            if (dto.Image != null)
            {
                var imageUrl = await _imageService.UploadImageAsync(dto.Image);
                var applianceImage = new ApplianceImage
                {
                    Url = imageUrl,
                    IsMain = false,
                    ApplianceId = appliance.Id
                };
                _context.ApplianceImages.Add(applianceImage);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Appliance updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAppliance(int id)
        {
            var appliance = await _context.Appliances.FindAsync(id);

            if (appliance == null)
                return NotFound(new { message = "Appliance not found" });

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null || currentUserId.Value != appliance.SellerId)
                return Forbid();

            _context.Appliances.Remove(appliance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appliance deleted successfully" });
        }

        // ==========================================
        // ✨ الفانكشن السحرية لاستخراج الـ ID بشكل آمن ✨
        // ==========================================
        private int? GetCurrentUserId()
        {
            // بتدور على الكليم اللي قيمته تنفع تتحول لرقم فقط وتتجاهل الإيميل
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}