using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using ReStore.API.DTOs;

namespace ReStore.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            var items = await _context.WishlistItems
                .Include(w => w.Appliance)
                    .ThenInclude(a => a.Images)
                .Include(w => w.Appliance)
                    .ThenInclude(a => a.Category)
                .Where(w => w.UserId == currentUserId.Value)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var applianceDtos = items.Select(w => new ApplianceDto
            {
                Id = w.Appliance.Id,
                Title = w.Appliance.Title,
                Description = w.Appliance.Description,
                Price = w.Appliance.Price,
                Condition = w.Appliance.Condition.ToString(),
                CategoryName = w.Appliance.Category?.Name,
                Status = w.Appliance.Status.ToString(),
                ImageUrl = w.Appliance.Images.FirstOrDefault(i => i.IsMain)?.Url != null
                           ? $"{baseUrl}{w.Appliance.Images.FirstOrDefault(i => i.IsMain)?.Url}"
                           : null,
                ImageUrls = w.Appliance.Images.Select(i => i.Url).ToList()
            }).ToList();

            return Ok(applianceDtos);
        }

        [HttpPost("{applianceId}")]
        public async Task<IActionResult> AddToWishlist(int applianceId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            var appliance = await _context.Appliances.FindAsync(applianceId);
            if (appliance == null)
                return NotFound(new { message = "Appliance not found." });

            var existing = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == currentUserId.Value && w.ApplianceId == applianceId);

            if (existing != null)
                return BadRequest(new { message = "Appliance already in wishlist." });

            var wishlistItem = new WishlistItem
            {
                UserId = currentUserId.Value,
                ApplianceId = applianceId,
                AddedDate = DateTime.UtcNow
            };

            _context.WishlistItems.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appliance added to wishlist." });
        }

        [HttpDelete("{applianceId}")]
        public async Task<IActionResult> RemoveFromWishlist(int applianceId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == currentUserId.Value && w.ApplianceId == applianceId);

            if (item == null)
                return NotFound(new { message = "Wishlist item not found." });

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appliance removed from wishlist." });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
