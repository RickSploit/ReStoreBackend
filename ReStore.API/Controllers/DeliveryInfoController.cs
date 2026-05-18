using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.DTOs;
using ReStore.API.Entities;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using System.Security.Claims;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryInfoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DeliveryInfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Get delivery info for an order
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderDeliveryInfo(int orderId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound(new { message = "Order not found." });

            // Allow access if user owns the order or is admin
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && order.BuyerId != userId.Value)
                return Forbid();

            var deliveryInfo = await _context.DeliveryInfo
                .Where(d => d.OrderId == orderId)
                .FirstOrDefaultAsync();

            if (deliveryInfo == null)
                return NotFound(new { message = "Delivery info not found." });

            var dto = new DeliveryInfoDto
            {
                Id = deliveryInfo.Id,
                Status = deliveryInfo.Status,
                Address = deliveryInfo.Address,
                City = deliveryInfo.City,
                TrackingNumber = deliveryInfo.TrackingNumber,
                OrderId = deliveryInfo.OrderId
            };

            return Ok(dto);
        }

        // POST: Create delivery info (typically done when order is placed)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDeliveryInfo([FromBody] CreateDeliveryInfoDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return NotFound(new { message = "Order not found." });

            var deliveryInfo = new DeliveryInfo
            {
                OrderId = dto.OrderId,
                Address = dto.Address,
                City = dto.City,
                Status = "Processing",
                TrackingNumber = string.Empty
            };

            _context.DeliveryInfo.Add(deliveryInfo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Delivery info created successfully!", deliveryInfoId = deliveryInfo.Id });
        }

        // PUT: Update delivery status/tracking
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDeliveryInfo(int id, [FromBody] UpdateDeliveryInfoDto dto)
        {
            var deliveryInfo = await _context.DeliveryInfo.FindAsync(id);
            if (deliveryInfo == null)
                return NotFound(new { message = "Delivery info not found." });

            deliveryInfo.Status = dto.Status;
            deliveryInfo.TrackingNumber = dto.TrackingNumber;

            _context.DeliveryInfo.Update(deliveryInfo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Delivery info updated successfully!" });
        }

        // GET: Admin get all delivery info
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDeliveryInfo()
        {
            var deliveryInfoList = await _context.DeliveryInfo
                .Include(d => d.Order)
                .ToListAsync();

            var dtos = deliveryInfoList.Select(d => new DeliveryInfoDto
            {
                Id = d.Id,
                Status = d.Status,
                Address = d.Address,
                City = d.City,
                TrackingNumber = d.TrackingNumber,
                OrderId = d.OrderId
            }).ToList();

            return Ok(dtos);
        }

        // Helper method to get current user ID
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
