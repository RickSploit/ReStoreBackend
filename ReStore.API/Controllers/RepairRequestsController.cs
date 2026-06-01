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
                .Include(r => r.Seller)
                .Include(r => r.Technician)
                .Where(r => r.SellerId == userId.Value)
                .ToListAsync();

            var requestDtos = requests.Select(r => new RepairRequestDto
            {
                Id = r.Id,
                ApplianceId = r.ApplianceId,
                ApplianceTitle = r.Appliance?.Title,
                IssuesDescription = r.IssuesDescription,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString(),
                SellerId = r.SellerId,
                SellerName = r.Seller?.Name,
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
                .Include(r => r.Seller)
                .Include(r => r.Appliance)
                .Include(r => r.Technician)
                .ToListAsync();

            var requestDtos = requests.Select(r => new RepairRequestDto
            {
                Id = r.Id,
                ApplianceId = r.ApplianceId,
                ApplianceTitle = r.Appliance?.Title,
                IssuesDescription = r.IssuesDescription,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString(),
                SellerId = r.SellerId,
                SellerName = r.Seller?.Name,
                TechnicianId = r.TechnicianId,
                TechnicianName = r.Technician?.Name
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
                .Include(r => r.Seller)
                .Include(r => r.Appliance)
                .Include(r => r.Technician)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound(new { message = "Repair request not found." });

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && request.SellerId != userId.Value && request.TechnicianId != userId.Value)
                return Forbid();

            var requestDto = new RepairRequestDto
            {
                Id = request.Id,
                ApplianceId = request.ApplianceId,
                ApplianceTitle = request.Appliance?.Title,
                IssuesDescription = request.IssuesDescription,
                RequestDate = request.RequestDate,
                Status = request.Status.ToString(),
                SellerId = request.SellerId,
                SellerName = request.Seller?.Name,
                TechnicianId = request.TechnicianId,
                TechnicianName = request.Technician?.Name
            };

            return Ok(requestDto);
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

            if (dto.TechnicianId.HasValue && request.Status == RepairRequestStatus.Pending)
            {
                var technician = await _context.Users.FindAsync(dto.TechnicianId.Value);
                if (technician == null)
                    return NotFound(new { message = "Technician not found." });

                request.TechnicianId = dto.TechnicianId.Value;
                request.Status = RepairRequestStatus.InProgress;
            }

            _context.RepairRequests.Update(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Repair request updated successfully!" });
        }

        // POST: Seller creates a repair request for their appliance
        [HttpPost]
        public async Task<IActionResult> CreateRepairRequest([FromBody] CreateRepairRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var appliance = await _context.Appliances.FindAsync(dto.ApplianceId);
            if (appliance == null)
                return NotFound(new { message = "Appliance not found." });

            if (appliance.SellerId != userId.Value)
                return Forbid();

            var request = new RepairRequest
            {
                SellerId = userId.Value,
                ApplianceId = dto.ApplianceId,
                IssuesDescription = dto.IssuesDescription,
                Status = RepairRequestStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            _context.RepairRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Repair request submitted successfully!", requestId = request.Id });
        }

        // GET: Technician views all pending repair requests
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRepairRequests()
        {
            var requests = await _context.RepairRequests
                .Include(r => r.Appliance)
                .Include(r => r.Seller)
                .Where(r => r.Status == RepairRequestStatus.Pending)
                .ToListAsync();

            var requestDtos = requests.Select(r => new RepairRequestDto
            {
                Id = r.Id,
                ApplianceId = r.ApplianceId,
                ApplianceTitle = r.Appliance?.Title,
                IssuesDescription = r.IssuesDescription,
                RequestDate = r.RequestDate,
                Status = r.Status.ToString(),
                SellerId = r.SellerId,
                SellerName = r.Seller?.Name,
                TechnicianId = r.TechnicianId,
                TechnicianName = r.Technician?.Name
            }).ToList();

            return Ok(requestDtos);
        }

        // PUT: Technician accepts a repair request
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptRepairRequest(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var request = await _context.RepairRequests.FindAsync(id);
            if (request == null)
                return NotFound(new { message = "Repair request not found." });

            if (request.Status != RepairRequestStatus.Pending)
                return BadRequest(new { message = "Request is not pending." });

            request.TechnicianId = userId.Value;
            request.Status = RepairRequestStatus.InProgress;

            _context.RepairRequests.Update(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Repair request accepted." });
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
