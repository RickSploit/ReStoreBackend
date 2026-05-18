using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.DTOs;
using ReStore.API.Entities;
using ReStore.Infrastructure.Data;
using System.Security.Claims;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComplaintsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ComplaintsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Get current user's complaints
        [HttpGet]
        public async Task<IActionResult> GetUserComplaints()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

var complaints = await _context.Complaints
                .Include(c => c.Order)
                .Where(c => c.UserId == userId.Value)
                .ToListAsync();

            var complaintDtos = complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                IssueType = c.IssueType,
                Description = c.Description,
                Status = c.Status,
                UserId = c.UserId,
                OrderId = c.OrderId
            }).ToList();

            return Ok(complaintDtos);
        }

        // GET: Get specific complaint
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComplaint(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var complaint = await _context.Complaints
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
                return NotFound(new { message = "Complaint not found." });

            // Allow access if user owns it or is admin
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && complaint.UserId != userId.Value)
                return Forbid();

            var complaintDto = new ComplaintDto
            {
                Id = complaint.Id,
                IssueType = complaint.IssueType,
                Description = complaint.Description,
                Status = complaint.Status,
                UserId = complaint.UserId,
                OrderId = complaint.OrderId
            };

            return Ok(complaintDto);
        }

        // POST: Create complaint
        [HttpPost]
        public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            // Verify order exists
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return NotFound(new { message = "Order not found." });

            var complaint = new Complaint
            {
                UserId = userId.Value,
                OrderId = dto.OrderId,
                IssueType = dto.IssueType,
                Description = dto.Description,
                Status = "Open"
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Complaint submitted successfully!", complaintId = complaint.Id });
        }

        // PUT: Admin updates complaint status
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateComplaint(int id, [FromBody] UpdateComplaintDto dto)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
                return NotFound(new { message = "Complaint not found." });

            complaint.Status = dto.Status;

            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Complaint updated successfully!" });
        }

        // GET: Admin get all complaints
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllComplaints()
        {
            var complaints = await _context.Complaints
                .Include(c => c.Order)
                .ToListAsync();

            var complaintDtos = complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                IssueType = c.IssueType,
                Description = c.Description,
                Status = c.Status,
                UserId = c.UserId,
                OrderId = c.OrderId
            }).ToList();

            return Ok(complaintDtos);
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
