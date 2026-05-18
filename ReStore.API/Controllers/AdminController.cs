using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.Entities;
using ReStore.Infrastructure.Data;
using System.Security.Claims;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Approve a technician account (Admin only)
        /// </summary>
        [HttpPut("approve-technician/{id}")]
        public async Task<IActionResult> ApproveTechnician(int id)
        {
            var profile = await _context.TechnicianProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(new { message = "Technician profile not found." });

            profile.IsApproved = true;
            profile.ApprovedDate = DateTime.UtcNow;

            _context.TechnicianProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Technician account approved successfully!" });
        }

        /// <summary>
        /// Reject a technician account (Admin only)
        /// </summary>
        [HttpPut("reject-technician/{id}")]
        public async Task<IActionResult> RejectTechnician(int id)
        {
            var profile = await _context.TechnicianProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(new { message = "Technician profile not found." });

            // Optionally: mark as rejected or delete
            profile.IsApproved = false;

            _context.TechnicianProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Technician account rejected." });
        }

        /// <summary>
        /// Get all pending technician requests (Admin only)
        /// </summary>
        [HttpGet("pending-technicians")]
        public async Task<IActionResult> GetPendingTechnicians()
        {
            var pendingTechnicians = await _context.TechnicianProfiles
                .Include(t => t.User)
                .Where(t => !t.IsApproved)
                .ToListAsync();

return Ok(pendingTechnicians.Select(t => new
            {
                ProfileId = t.Id,
                UserId = t.User.Id,
                t.User.Name,
                t.User.Email,
                t.Specialty,
                t.ExperienceYears,
                t.Rating
            }));
        }

        /// <summary>
        /// Get all approved technicians (Admin only)
        /// </summary>
        [HttpGet("approved-technicians")]
        public async Task<IActionResult> GetApprovedTechnicians()
        {
            var approvedTechnicians = await _context.TechnicianProfiles
                .Include(t => t.User)
                .Where(t => t.IsApproved)
                .ToListAsync();

return Ok(approvedTechnicians.Select(t => new
            {
                ProfileId = t.Id,
                UserId = t.User.Id,
                t.User.Name,
                t.User.Email,
                t.Specialty,
                t.ExperienceYears,
                t.Rating,
                t.ApprovedDate
            }));
        }
    }
}
