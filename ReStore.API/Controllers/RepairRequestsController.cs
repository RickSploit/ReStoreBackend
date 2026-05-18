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
    [Authorize]
    public class RepairRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RepairRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Get current user's repair requests
        [HttpGet]
        public async Task<IActionResult> GetUserRepairRequests()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var requests = await _context.RepairRequests
                .Include(r => r.Buyer)
                .Include(r => r.Technician)
                .Where(r => r.BuyerId == userId.Value)
                .ToListAsync();

            var requestDtos = requests.Select(r => new RepairRequestDto
            {
                Id = r.Id,
                DeviceType = r.DeviceType,
                ProblemDescription = r.ProblemDescription,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString(),
                BuyerId = r.BuyerId,
                BuyerName = r.Buyer?.Name ?? string.Empty,
                TechnicianId = r.TechnicianId,
                TechnicianName = r.Technician?.Name
            }).ToList();

            return Ok(requestDtos);
        }

        // GET: Get all repair requests (technician/admin)
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> GetAllRepairRequests()
        {
            var requests = await _context.RepairRequests
                .Include(r => r.Buyer)
                .Include(r => r.Technician)
                .ToListAsync();

            var requestDtos = requests.Select(r => new RepairRequestDto
            {
                Id = r.Id,
                DeviceType = r.DeviceType,
                ProblemDescription = r.ProblemDescription,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString(),
                BuyerId = r.BuyerId,
                BuyerName = r.Buyer?.Name ?? string.Empty,
                TechnicianId = r.TechnicianId,
                TechnicianName = r.Technician?.Name
            }).ToList();

            return Ok(requestDtos);
        }

        // GET: Get available repair requests for technicians
        [HttpGet("available")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> GetAvailableRepairRequests()
        {
            var requests = await _context.RepairRequests
                .Include(r => r.Buyer)
                .Where(r => r.Status == RepairRequestStatus.Pending)
                .ToListAsync();

            var requestDtos = requests.Select(r => new RepairRequestDto
            {
                Id = r.Id,
                DeviceType = r.DeviceType,
                ProblemDescription = r.ProblemDescription,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString(),
                BuyerId = r.BuyerId,
                BuyerName = r.Buyer?.Name ?? string.Empty
            }).ToList();

            return Ok(requestDtos);
        }

        // GET: Get specific repair request
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepairRequest(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var request = await _context.RepairRequests
                .Include(r => r.Buyer)
                .Include(r => r.Technician)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound(new { message = "Repair request not found." });

            // Allow access if user owns it, assigned technician, or is admin
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && request.BuyerId != userId.Value && request.TechnicianId != userId.Value)
                return Forbid();

            var requestDto = new RepairRequestDto
            {
                Id = request.Id,
                DeviceType = request.DeviceType,
                ProblemDescription = request.ProblemDescription,
                RequestDate = request.RequestDate,
                Status = request.Status.ToString(),
                BuyerId = request.BuyerId,
                BuyerName = request.Buyer?.Name ?? string.Empty,
                TechnicianId = request.TechnicianId,
                TechnicianName = request.Technician?.Name
            };

            return Ok(requestDto);
        }

        // POST: Create a repair request
        [HttpPost]
        public async Task<IActionResult> CreateRepairRequest([FromBody] CreateRepairRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var request = new RepairRequest
            {
                DeviceType = dto.DeviceType,
                ProblemDescription = dto.ProblemDescription,
                BuyerId = userId.Value,
                Status = RepairRequestStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            _context.RepairRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Repair request submitted successfully!", requestId = request.Id });
        }

        // PUT: Technician accepts/completes repair request
        [HttpPut("{id}")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> UpdateRepairRequest(int id, [FromBody] UpdateRepairRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var request = await _context.RepairRequests.FindAsync(id);
            if (request == null)
                return NotFound(new { message = "Repair request not found." });

            if (dto.Status != null && Enum.TryParse<RepairRequestStatus>(dto.Status, true, out var newStatus))
            {
                request.Status = newStatus;
            }

            // Technician accepts the request
            if (dto.TechnicianId.HasValue && request.Status == RepairRequestStatus.Pending)
            {
                var technician = await _context.Users.FindAsync(dto.TechnicianId.Value);
                if (technician == null)
                    return NotFound(new { message = "Technician not found." });

                request.TechnicianId = dto.TechnicianId.Value;
                request.Status = RepairRequestStatus.Accepted;
            }

            _context.RepairRequests.Update(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Repair request updated successfully!" });
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
