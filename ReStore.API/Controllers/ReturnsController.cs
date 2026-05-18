using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.DTOs;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using System.Security.Claims;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReturnsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Submit a return request
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReturnRequest([FromBody] ReturnRequestCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            // Verify order exists
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return NotFound(new { message = "Order not found." });

            // Verify user owns the order
            if (order.BuyerId != userId.Value)
                return Forbid();

            var returnRequest = new ReturnRequest
            {
                OrderId = dto.OrderId,
                UserId = userId.Value,
                Reason = dto.Reason,
                Description = dto.Description,
                Status = ReturnStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            _context.ReturnRequests.Add(returnRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Return request submitted successfully!", returnRequestId = returnRequest.Id });
        }

        // GET: Get current user's return requests
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserReturns()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var returns = await _context.ReturnRequests
                .Include(r => r.Order)
                .Where(r => r.UserId == userId.Value)
                .ToListAsync();

            var returnDtos = returns.Select(r => new ReturnRequestDto
            {
                Id = r.Id,
                OrderId = r.OrderId,
                UserId = r.UserId,
                Reason = r.Reason,
                Description = r.Description,
                Status = r.Status.ToString(),
                RefundAmount = r.RefundAmount,
                AdminNote = r.AdminNote,
                RequestDate = r.RequestDate,
                ProcessedDate = r.ProcessedDate
            }).ToList();

            return Ok(returnDtos);
        }

        // GET: Get specific return request by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReturnRequest(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (returnRequest == null)
                return NotFound(new { message = "Return request not found." });

            // Allow access if user owns the request or is admin
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && returnRequest.UserId != userId.Value)
                return Forbid();

            var returnDto = new ReturnRequestDto
            {
                Id = returnRequest.Id,
                OrderId = returnRequest.OrderId,
                UserId = returnRequest.UserId,
                UserName = returnRequest.User?.Name,
                Reason = returnRequest.Reason,
                Description = returnRequest.Description,
                Status = returnRequest.Status.ToString(),
                RefundAmount = returnRequest.RefundAmount,
                AdminNote = returnRequest.AdminNote,
                RequestDate = returnRequest.RequestDate,
                ProcessedDate = returnRequest.ProcessedDate
            };

            return Ok(returnDto);
        }

        // PUT: Admin updates return status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateReturnStatus(int id, [FromBody] ReturnRequestUpdateDto dto)
        {
            var returnRequest = await _context.ReturnRequests.FindAsync(id);
            if (returnRequest == null)
                return NotFound(new { message = "Return request not found." });

            // Parse and update status
            if (Enum.TryParse<ReturnStatus>(dto.Status, true, out var newStatus))
            {
                returnRequest.Status = newStatus;
            }
            else
            {
                return BadRequest(new { message = "Invalid status. Valid values: Pending, Approved, Rejected, Processed" });
            }

            returnRequest.RefundAmount = dto.RefundAmount;
            returnRequest.AdminNote = dto.AdminNote;
            returnRequest.ProcessedDate = DateTime.UtcNow;

            _context.ReturnRequests.Update(returnRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Return request status updated successfully!" });
        }

        // GET: Admin get all return requests
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReturns()
        {
            var returns = await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.User)
                .ToListAsync();

            var returnDtos = returns.Select(r => new ReturnRequestDto
            {
                Id = r.Id,
                OrderId = r.OrderId,
                UserId = r.UserId,
                UserName = r.User?.Name,
                Reason = r.Reason,
                Description = r.Description,
                Status = r.Status.ToString(),
                RefundAmount = r.RefundAmount,
                AdminNote = r.AdminNote,
                RequestDate = r.RequestDate,
                ProcessedDate = r.ProcessedDate
            }).ToList();

            return Ok(returnDtos);
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
